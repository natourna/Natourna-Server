using System.Text.Json;
using BuildingManagement.Constants.Cycle;
using BuildingManagement.Constants.Error;
using BuildingManagement.Constants.Log;
using BuildingManagement.Exceptions;
using BuildingManagement.Interfaces.Api;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Interfaces.Services;
using BuildingManagement.Models.Api.Requests.Cycle;
using BuildingManagement.Models.Api.Requests.Payment;
using BuildingManagement.Models.Entities;

namespace BuildingManagement.Services.Api
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

        public async Task<List<CycleEntity>> GetAllCyclesAsync()
        {
            return await _cycleContextManager.GetAllAsync();
        }

        public async Task<CycleEntity?> GetCycleByIdAsync(int id)
        {
            return await _cycleContextManager.GetByIdAsync(id);
        }

        public async Task<CycleEntity> CreateCycleAsync(CycleRequest request)
        {
            try
            {
                _logger.LogInformation("Creating cycle - Label: {Label}, Cycle: {CycleType}, Amount: {Amount}",
                    request.Label, request.Cycle, request.Amount);

                if (request.StartDate.Date > request.EndDate.Date)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Cycle.InvalidDateRange(request.StartDate, request.EndDate);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.CYCLE_CREATE_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.CYCLE_CREATE_ERROR, userMessage, technicalDetails);
                }

                if (request.BalanceAllocations == null || !request.BalanceAllocations.Any())
                {
                    _logger.LogWarning("[{ErrorCode}] Balance allocations are required", ErrorCodes.CYCLE_CREATE_ERROR);
                    throw new ApiException(ErrorCodes.CYCLE_CREATE_ERROR, "Balance allocations are required for all payments", "BalanceAllocations: null or empty");
                }

                var totalPercentage = request.BalanceAllocations.Sum(a => a.Percentage);
                if (totalPercentage != 100)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Cycle.InvalidBalanceAllocations(totalPercentage);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.CYCLE_CREATE_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.CYCLE_CREATE_ERROR, userMessage, technicalDetails);
                }

                foreach (var allocation in request.BalanceAllocations)
                {
                    var balances = await _balanceContextManager.GetAllAsync(balanceId: allocation.BalanceId);
                    if (!balances.Any())
                    {
                        var (userMessage, technicalDetails) = ErrorMessageBuilder.Balance.NotFound(allocation.BalanceId);
                        _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage);
                        throw new ApiException(ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage, technicalDetails);
                    }
                }

                var allocations = request.BalanceAllocations.Select(a => new
                {
                    balanceId = a.BalanceId,
                    percentage = a.Percentage
                }).ToList();

                var balanceAllocationsJson = JsonSerializer.Serialize(allocations);

                var cycle = new CycleEntity(request.Label, request.Cycle, request.StartDate.Date, request.EndDate.Date, request.Amount)
                {
                    Description = request.Description,
                    ApartmentIdsCsv = request.ApartmentIds == null || request.ApartmentIds.Count == 0 ? null : string.Join(',', request.ApartmentIds),
                    IsActive = true,
                    BalanceAllocationsJson = balanceAllocationsJson
                };


                var createdCycle = await _cycleContextManager.CreateAsync(cycle);

                await GeneratePaymentsForCycle(createdCycle.Id, request);

                await _auditService.LogAsync(LogAction.Create, "Cycle", createdCycle.Id, null, new
                {
                    createdCycle.Label,
                    createdCycle.Cycle,
                    createdCycle.Amount,
                    createdCycle.StartDate,
                    createdCycle.EndDate,
                    ApartmentCount = request.ApartmentIds?.Count ?? 0
                });

                _logger.LogInformation("Successfully created cycle {CycleId} with label '{Label}' and {AllocationCount} balance allocations", createdCycle.Id, createdCycle.Label, request.BalanceAllocations.Count);

                return await _cycleContextManager.GetByIdAsync(createdCycle.Id) ?? createdCycle;
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
                _logger.LogInformation("Generating payments for cycle {CycleId}", cycleId);

                List<int> apartmentIds;
                if (request.ApartmentIds != null && request.ApartmentIds.Any())
                {
                    apartmentIds = request.ApartmentIds;
                }
                else
                {
                    var apartments = await _apartmentContextManager.GetAllAsync(isActive: true);
                    apartmentIds = apartments.Select(a => a.Id).ToList();
                }

                var occurrences = CalculatePaymentOccurrences(request.Cycle, request.StartDate, request.EndDate);

                _logger.LogInformation("Creating {OccurrenceCount} payment occurrences for {ApartmentCount} apartments", occurrences.Count, apartmentIds.Count);

                foreach (var aptId in apartmentIds)
                {
                    foreach (var occurrence in occurrences)
                    {
                        var payment = new PaymentEntity(request.Amount, aptId)
                        {
                            PaymentDate = null,
                            DueDate = occurrence,
                            IsPaid = false,
                            CycleId = cycleId
                        };

                        var createdPayment = await _paymentContextManager.CreateAsync(payment);

                        await ApplyBalanceAllocationsToPayment(createdPayment, request.BalanceAllocations);
                    }
                }

                _logger.LogInformation("Successfully created {PaymentCount} payments with allocations for cycle {CycleId}", occurrences.Count * apartmentIds.Count, cycleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate payments for cycle {CycleId}", cycleId);
                throw;
            }
        }

        private List<DateTime> CalculatePaymentOccurrences(PaymentCycle cycleType, DateTime startDate, DateTime endDate)
        {
            var occurrences = new List<DateTime>();
            var current = startDate.Date;
            var end = endDate.Date;

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
                var paymentAllocations = new List<PaymentAllocationEntity>();

                foreach (var allocation in allocations)
                {
                    var allocatedAmount = payment.Amount * (allocation.Percentage / 100m);

                    var paymentAllocation = new PaymentAllocationEntity
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

        public async Task<CycleEntity?> UpdateCycleAsync(int id, CycleEntity cycle)
        {
            var existing = await GetCycleByIdAsync(id);
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

            var updated = await _cycleContextManager.UpdateAsync(id, cycle);

            if (updated != null)
            {
                await _auditService.LogAsync(LogAction.Update, "Cycle", id, oldValues, new
                {
                    updated.Label,
                    updated.Amount,
                    updated.IsActive
                });
            }

            return updated;
        }

        public async Task<bool> DeleteCycleAsync(int id)
        {
            var existing = await GetCycleByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            await _auditService.LogAsync(LogAction.Delete, "Cycle", id, new
            {
                existing.Label,
                existing.Amount
            }, null);

            return await _cycleContextManager.DeleteAsync(id);
        }
    }
}
