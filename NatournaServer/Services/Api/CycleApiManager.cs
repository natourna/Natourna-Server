using NatournaServer.Constants.Cycle;
using NatournaServer.Constants.Error;
using NatournaServer.Constants.Log;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Models.Api.Requests.Cycle;
using NatournaServer.Models.Api.Requests.Payment;
using NatournaServer.Models.Api.Response.Cycle;
using NatournaServer.Models.Entities;
using System.Text.Json;

namespace NatournaServer.Services.Api
{
    public class CycleApiManager : ICycleApiManager
    {
        private readonly ICycleContextManager _cycleContextManager;
        private readonly IPaymentContextManager _paymentContextManager;
        private readonly IPaymentAllocationContextManager _paymentAllocationContextManager;
        private readonly IBalanceContextManager _balanceContextManager;
        private readonly IApartmentContextManager _apartmentContextManager;
        private readonly ITransactionManager _transactionManager;
        private readonly IAuditService _auditService;
        private readonly ILogger<CycleApiManager> _logger;

        public CycleApiManager(ICycleContextManager cycleContextManager, IPaymentContextManager paymentContextManager, IPaymentAllocationContextManager paymentAllocationContextManager, IBalanceContextManager balanceContextManager, IApartmentContextManager apartmentContextManager, ITransactionManager transactionManager, IAuditService auditService, ILogger<CycleApiManager> logger)
        {
            _cycleContextManager = cycleContextManager;
            _paymentContextManager = paymentContextManager;
            _paymentAllocationContextManager = paymentAllocationContextManager;
            _balanceContextManager = balanceContextManager;
            _apartmentContextManager = apartmentContextManager;
            _transactionManager = transactionManager;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<List<CycleResponse>> GetAllCyclesAsync()
        {
            List<CycleEntity> cycles = await _cycleContextManager.GetAllAsync();
            return cycles.Select(MapToResponse).ToList();
        }

        public async Task<CycleResponse?> GetCycleByIdAsync(int id)
        {
            CycleEntity? cycle = await _cycleContextManager.GetByIdAsync(id);
            return cycle != null ? MapToResponse(cycle) : null;
        }

        public async Task<CycleResponse> CreateCycleAsync(CycleRequest request)
        {
            try
            {
                _logger.LogInformation("Creating cycle - Label: {Label}, Cycle: {CycleType}, Amount: {Amount}",
                    request.Label, request.Cycle, request.Amount);

                if (request.StartDate.Date > request.EndDate.Date)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Cycle.InvalidDateRange(request.StartDate, request.EndDate);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.CYCLE_CREATE_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.CYCLE_CREATE_ERROR, userMessage, technicalDetails);
                }

                if (request.BalanceAllocations == null || request.BalanceAllocations.Count == 0)
                {
                    _logger.LogWarning("[{ErrorCode}] Balance allocations are required", ErrorCodes.CYCLE_CREATE_ERROR);
                    throw new ApiException(ErrorCodes.CYCLE_CREATE_ERROR, "Balance allocations are required for all payments", "BalanceAllocations: null or empty");
                }

                decimal totalPercentage = request.BalanceAllocations.Sum(a => a.Percentage);

                if (totalPercentage != 100)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Cycle.InvalidBalanceAllocations(totalPercentage);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.CYCLE_CREATE_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.CYCLE_CREATE_ERROR, userMessage, technicalDetails);
                }

                foreach (int balanceId in request.BalanceAllocations.Select(x => x.BalanceId))
                {
                    BalanceEntity? balance = await _balanceContextManager.GetByIdAsync(balanceId);
                    if (balance == null)
                    {
                        (string userMessage, string technicalDetails) = ErrorMessageBuilder.Balance.NotFound(balanceId);
                        _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage);
                        throw new ApiException(ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage, technicalDetails, statusCode: 404);
                    }
                }

                var allocations = request.BalanceAllocations.Select(a => new { balanceId = a.BalanceId, percentage = a.Percentage }).ToList();

                string balanceAllocationsJson = JsonSerializer.Serialize(allocations);

                CycleEntity cycle = new (request.Label, request.Cycle, request.StartDate.Date, request.EndDate.Date, request.Amount)
                {
                    Description = request.Description,
                    ApartmentIdsCsv = request.ApartmentIds == null || request.ApartmentIds.Count == 0 ? null : string.Join(',', request.ApartmentIds),
                    IsActive = true,
                    BalanceAllocationsJson = balanceAllocationsJson
                };


                List<int> apartmentIds = await ResolveTargetApartmentIdsAsync(request.ApartmentIds);
                List<DateTime> occurrences = CalculatePaymentOccurrences(request.Cycle, request.StartDate, request.EndDate);

                // The cycle and every payment it expands into must land together or not at all
                CycleEntity createdCycle = await _transactionManager.ExecuteInTransactionAsync(async () =>
                {
                    CycleEntity created = await _cycleContextManager.CreateAsync(cycle);
                    await GeneratePaymentsForCycleAsync(created.Id, apartmentIds, occurrences, request);
                    return created;
                });

                await _auditService.LogAsync(LogAction.Create, "Cycle", createdCycle.Id, null, new { createdCycle.Label, createdCycle.Cycle, createdCycle.Amount, createdCycle.StartDate, createdCycle.EndDate, ApartmentCount = apartmentIds.Count });

                _logger.LogInformation("Successfully created cycle {CycleId} with label '{Label}' and {AllocationCount} balance allocations", createdCycle.Id, createdCycle.Label, request.BalanceAllocations.Count);

                return MapToResponse(await _cycleContextManager.GetByIdAsync(createdCycle.Id) ?? createdCycle);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to create cycle", ErrorCodes.CYCLE_CREATE_ERROR);
                throw new ApiException(ErrorCodes.CYCLE_CREATE_ERROR, "Failed to create cycle", $"Label: {request.Label}", ex, statusCode: 500);
            }
        }

        /// <summary>Resolves the apartments a cycle targets: the requested ids (all must exist) or every active apartment.</summary>
        private async Task<List<int>> ResolveTargetApartmentIdsAsync(List<int>? requestedIds)
        {
            if (requestedIds == null || requestedIds.Count == 0)
            {
                List<ApartmentEntity> activeApartments = await _apartmentContextManager.GetAllAsync(isActive: true);
                return activeApartments.Select(a => a.Id).ToList();
            }

            List<ApartmentEntity> apartments = await _apartmentContextManager.GetAllAsync();
            HashSet<int> knownIds = apartments.Select(a => a.Id).ToHashSet();
            List<int> missing = requestedIds.Where(id => !knownIds.Contains(id)).Distinct().ToList();

            if (missing.Count > 0)
            {
                string missingList = string.Join(", ", missing);
                _logger.LogWarning("[{ErrorCode}] Cycle references unknown apartments: {Missing}", ErrorCodes.APARTMENT_NOT_FOUND_ERROR, missingList);
                throw new ApiException(ErrorCodes.APARTMENT_NOT_FOUND_ERROR, $"These apartments were not found: {missingList}", $"Missing apartment ids: {missingList}", statusCode: 404);
            }

            return requestedIds.Distinct().ToList();
        }

        /// <summary>Expands a cycle into its payments and allocations, inserted as two batches.</summary>
        private async Task GeneratePaymentsForCycleAsync(int cycleId, List<int> apartmentIds, List<DateTime> occurrences, CycleRequest request)
        {
            _logger.LogInformation("Creating {OccurrenceCount} payment occurrences for {ApartmentCount} apartments (cycle {CycleId})", occurrences.Count, apartmentIds.Count, cycleId);

            List<PaymentEntity> payments = apartmentIds
                .SelectMany(aptId => occurrences.Select(occurrence => new PaymentEntity($"{request.Label} - {occurrence:MMMM yyyy}", request.Amount, aptId)
                {
                    PaymentDate = null,
                    DueDate = occurrence,
                    IsPaid = false,
                    CycleId = cycleId
                }))
                .ToList();

            if (payments.Count == 0)
            {
                return;
            }

            await _paymentContextManager.CreateRangeAsync(payments);

            List<PaymentAllocationEntity> allocations = payments
                .SelectMany(payment => request.BalanceAllocations.Select(allocation => new PaymentAllocationEntity
                {
                    PaymentId = payment.Id,
                    BalanceId = allocation.BalanceId,
                    Percentage = allocation.Percentage,
                    AllocatedAmount = payment.Amount * (allocation.Percentage / 100m)
                }))
                .ToList();

            await _paymentAllocationContextManager.CreateRangeAsync(allocations);

            _logger.LogInformation("Successfully created {PaymentCount} payments with allocations for cycle {CycleId}", payments.Count, cycleId);
        }

        private static List<DateTime> CalculatePaymentOccurrences(PaymentCycle cycleType, DateTime startDate, DateTime endDate)
        {
            List<DateTime> occurrences = new ();
            DateTime current = startDate.Date;
            DateTime end = endDate.Date;

            switch (cycleType)
            {
                case PaymentCycle.Monthly:
                    while (current <= end)
                    {
                        occurrences.Add(current);
                        current = current.AddMonths(1);
                    }
                    break;

                case PaymentCycle.Quarterly:
                    while (current <= end)
                    {
                        occurrences.Add(current);
                        current = current.AddMonths(3);
                    }
                    break;

                case PaymentCycle.SemiAnnual:
                    while (current <= end)
                    {
                        occurrences.Add(current);
                        current = current.AddMonths(6);
                    }
                    break;

                case PaymentCycle.Annual:
                    while (current <= end)
                    {
                        occurrences.Add(current);
                        current = current.AddYears(1);
                    }
                    break;

                case PaymentCycle.Weekly:
                    while (current <= end)
                    {
                        occurrences.Add(current);
                        current = current.AddDays(7);
                    }
                    break;

                case PaymentCycle.OneTime:
                    if (current <= end)
                    {
                        occurrences.Add(current);
                    }
                    break;
            }

            return occurrences;
        }


        public async Task<CycleResponse?> UpdateCycleAsync(int id, CycleUpdateRequest cycle)
        {
            CycleEntity? existing = await _cycleContextManager.GetByIdAsync(id);

            if (existing == null)
            {
                return null;
            }

            CycleEntity cycleEntity = new(cycle.Label, cycle.Cycle, cycle.StartDate, cycle.EndDate, cycle.Amount)
            {
                Description = cycle.Description,
                IsActive = cycle.IsActive,
                ApartmentIdsCsv = existing.ApartmentIdsCsv,
                BalanceAllocationsJson = existing.BalanceAllocationsJson
            };

            var oldValues = new
            {
                existing.Label,
                existing.Amount,
                existing.IsActive
            };

            CycleEntity? updated = await _cycleContextManager.UpdateAsync(id, cycleEntity);

            if (updated == null)
            {
                return null;
            }

            await _auditService.LogAsync(LogAction.Update, "Cycle", id, oldValues, new { updated.Label, updated.Amount, updated.IsActive });

            return MapToResponse(updated);
        }

        public async Task<bool> DeleteCycleAsync(int id)
        {
            CycleEntity? existing = await _cycleContextManager.GetByIdAsync(id);

            if (existing == null)
            {
                return false;
            }

            // The Cycle->Payments FK is Restrict; fail with a clear 409 instead of a raw database error
            if (await _paymentContextManager.AnyAsync(cycleId: id))
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Reference.InUse("Cycle", id, "payments");
                _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.CYCLE_HAS_PAYMENTS_ERROR, userMessage);
                throw new ApiException(ErrorCodes.CYCLE_HAS_PAYMENTS_ERROR, userMessage, technicalDetails, statusCode: 409);
            }

            await _auditService.LogAsync(LogAction.Delete, "Cycle", id, new { existing.Label, existing.Amount }, null);

            return await _cycleContextManager.DeleteAsync(id);
        }

        private static CycleResponse MapToResponse(CycleEntity cycle)
        {
            return new CycleResponse
            {
                Id = cycle.Id,
                Label = cycle.Label,
                Description = cycle.Description,
                PaymentCycle = cycle.Cycle.ToString(),
                StartDate = cycle.StartDate,
                EndDate = cycle.EndDate,
                ApartmentIds = cycle.ApartmentIdsCsv,
                Amount = cycle.Amount,
                IsActive = cycle.IsActive,
                BalanceAllocations = cycle.BalanceAllocationsJson
            };
        }
    }
}
