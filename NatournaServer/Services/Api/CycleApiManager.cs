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
using NatournaServer.Models.Api.Response.Paging;
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
        private readonly IAuditService _auditService;
        private readonly ILogger<CycleApiManager> _logger;

        public CycleApiManager(ICycleContextManager cycleContextManager, IPaymentContextManager paymentContextManager, IPaymentAllocationContextManager paymentAllocationContextManager, IBalanceContextManager balanceContextManager, IApartmentContextManager apartmentContextManager, IAuditService auditService, ILogger<CycleApiManager> logger)
        {
            _cycleContextManager = cycleContextManager;
            _paymentContextManager = paymentContextManager;
            _paymentAllocationContextManager = paymentAllocationContextManager;
            _balanceContextManager = balanceContextManager;
            _apartmentContextManager = apartmentContextManager;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<PagedResponse<CycleResponse>> GetCyclesAsync(int page, int pageSize)
        {
            (List<CycleEntity> items, int totalCount) = await _cycleContextManager.GetPagedAsync(page, pageSize);

            return new PagedResponse<CycleResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<CycleResponse?> GetActiveCycleAsync()
        {
            CycleEntity? cycle = await _cycleContextManager.GetActiveAsync();
            return cycle != null ? MapToResponse(cycle) : null;
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
                        throw new ApiException(ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage, technicalDetails);
                    }
                }

                if (request.ApartmentIds != null)
                {
                    foreach (int apartmentId in request.ApartmentIds)
                    {
                        ApartmentEntity? apartment = await _apartmentContextManager.GetByIdAsync(apartmentId);
                        if (apartment == null)
                        {
                            throw new ApiException(ErrorCodes.PAYMENT_APARTMENT_INVALID_ERROR, "The requested apartment does not exist", $"ApartmentId: {apartmentId}");
                        }
                    }
                }

                var allocations = request.BalanceAllocations.Select(a => new { balanceId = a.BalanceId, percentage = a.Percentage }).ToList();

                string balanceAllocationsJson = JsonSerializer.Serialize(allocations);

                CycleEntity cycle = new(request.Label, request.Cycle, request.StartDate.Date, request.EndDate.Date, request.Amount)
                {
                    Description = request.Description,
                    ApartmentIdsCsv = request.ApartmentIds == null || request.ApartmentIds.Count == 0 ? null : string.Join(',', request.ApartmentIds),
                    IsActive = true,
                    BalanceAllocationsJson = balanceAllocationsJson
                };

                CycleEntity createdCycle = await _cycleContextManager.CreateAsync(cycle);

                await GeneratePaymentsForCycle(createdCycle.Id, request);

                await _auditService.LogAsync(LogAction.Create, "Cycle", createdCycle.Id, null, new { createdCycle.Label, createdCycle.Cycle, createdCycle.Amount, createdCycle.StartDate, createdCycle.EndDate, ApartmentCount = request.ApartmentIds?.Count ?? 0 });

                _logger.LogInformation("Created cycle {CycleId} with label '{Label}' and {AllocationCount} balance allocations", createdCycle.Id, createdCycle.Label, request.BalanceAllocations.Count);

                CycleEntity? reloaded = await _cycleContextManager.GetByIdAsync(createdCycle.Id);

                return MapToResponse(reloaded ?? createdCycle);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to create cycle", ErrorCodes.CYCLE_CREATE_ERROR);
                throw new ApiException(ErrorCodes.CYCLE_CREATE_ERROR, "Failed to create cycle", $"Label: {request.Label}", ex);
            }
        }

        private async Task GeneratePaymentsForCycle(int cycleId, CycleRequest request)
        {
            try
            {
                List<int> apartmentIds;
                if (request.ApartmentIds != null && request.ApartmentIds.Count > 0)
                {
                    apartmentIds = request.ApartmentIds;
                }
                else
                {
                    List<ApartmentEntity> apartments = await _apartmentContextManager.GetAllAsync(isActive: true);
                    apartmentIds = apartments.Select(a => a.Id).ToList();
                }

                List<DateTime> occurrences = CalculatePaymentOccurrences(request.Cycle, request.StartDate, request.EndDate);

                _logger.LogInformation("Creating {OccurrenceCount} payment occurrences for {ApartmentCount} apartments", occurrences.Count, apartmentIds.Count);

                foreach (int aptId in apartmentIds)
                {
                    foreach (DateTime occurrence in occurrences)
                    {
                        string label = $"{request.Label} - {occurrence:MMMM yyyy}";
                        PaymentEntity payment = new(label, request.Amount, aptId)
                        {
                            PaymentDate = null,
                            DueDate = occurrence,
                            IsPaid = false,
                            CycleId = cycleId
                        };

                        PaymentEntity createdPayment = await _paymentContextManager.CreateAsync(payment);

                        await ApplyBalanceAllocationsToPayment(createdPayment, request.BalanceAllocations);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate payments for cycle {CycleId}", cycleId);
                throw;
            }
        }

        private static List<DateTime> CalculatePaymentOccurrences(PaymentCycle cycleType, DateTime startDate, DateTime endDate)
        {
            List<DateTime> occurrences = new();
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

        private async Task ApplyBalanceAllocationsToPayment(PaymentEntity payment, List<PaymentAllocationRequest> allocations)
        {
            try
            {
                List<PaymentAllocationEntity> paymentAllocations = new();

                foreach (PaymentAllocationRequest allocation in allocations)
                {
                    decimal allocatedAmount = payment.Amount * (allocation.Percentage / 100m);

                    PaymentAllocationEntity paymentAllocation = new()
                    {
                        PaymentId = payment.Id,
                        BalanceId = allocation.BalanceId,
                        Percentage = allocation.Percentage,
                        AllocatedAmount = allocatedAmount
                    };

                    paymentAllocations.Add(paymentAllocation);
                }

                await _paymentAllocationContextManager.CreateRangeAsync(paymentAllocations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to apply balance allocations to payment {PaymentId}", payment.Id);
                throw;
            }
        }

        public async Task<CycleResponse?> UpdateCycleAsync(int id, CycleUpdateRequest request)
        {
            CycleEntity? existing = await _cycleContextManager.GetByIdAsync(id);

            if (existing == null)
            {
                return null;
            }

            var oldValues = new
            {
                existing.Label,
                existing.Amount,
                existing.IsActive
            };

            CycleEntity? updated = await _cycleContextManager.UpdateAsync(id, request.Label, request.Description, request.IsActive);

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
