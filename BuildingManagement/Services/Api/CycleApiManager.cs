using System.Text.Json;
using BuildingManagement.Constants;
using BuildingManagement.Constants.Cycle;
using BuildingManagement.Exceptions;
using BuildingManagement.Interfaces.Api;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Models.Entities;
using BuildingManagement.Models.Requests.Cycle;
using BuildingManagement.Models.Requests.Payment;
using Microsoft.Extensions.Logging;

namespace BuildingManagement.Services.Api
{
    public class CycleApiManager : ICycleApiManager
    {
        private readonly ICycleContextManager _cycleContextManager;
        private readonly IPaymentContextManager _paymentContextManager;
        private readonly IPaymentAllocationContextManager _paymentAllocationContextManager;
        private readonly IBalanceContextManager _balanceContextManager;
        private readonly IApartmentContextManager _apartmentContextManager;
        private readonly ILogger<CycleApiManager> _logger;

        public CycleApiManager(ICycleContextManager cycleContextManager, IPaymentContextManager paymentContextManager, IPaymentAllocationContextManager paymentAllocationContextManager, IBalanceContextManager balanceContextManager, IApartmentContextManager apartmentContextManager, ILogger<CycleApiManager> logger)
        {
            _cycleContextManager = cycleContextManager;
            _paymentContextManager = paymentContextManager;
            _paymentAllocationContextManager = paymentAllocationContextManager;
            _balanceContextManager = balanceContextManager;
            _apartmentContextManager = apartmentContextManager;
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

        /// <summary>
        /// Creates a payment cycle and generates all payments for the specified period and apartments.
        /// If no apartments specified, generates payments for ALL active apartments.
        /// All payments will have balance allocations as specified in the request.
        /// </summary>
        public async Task<CycleEntity> CreateCycleAsync(CycleRequest request)
        {
            try
            {
                _logger.LogInformation("Creating cycle - Label: {Label}, Cycle: {CycleType}, Amount: {Amount}", 
                    request.Label, request.Cycle, request.Amount);

                // Validate date range
                if (request.StartDate.Date > request.EndDate.Date)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Cycle.InvalidDateRange(request.StartDate, request.EndDate);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.CYCLE_CREATE_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.CYCLE_CREATE_ERROR, userMessage, technicalDetails);
                }

                // Validate balance allocations (required - should already be validated by model but double-check)
                if (request.BalanceAllocations == null || !request.BalanceAllocations.Any())
                {
                    _logger.LogWarning("[{ErrorCode}] Balance allocations are required", ErrorCodes.CYCLE_CREATE_ERROR);
                    throw new ApiException(ErrorCodes.CYCLE_CREATE_ERROR,
                        "Balance allocations are required for all payments",
                        "BalanceAllocations: null or empty");
                }

                var totalPercentage = request.BalanceAllocations.Sum(a => a.Percentage);
                if (totalPercentage != 100)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Cycle.InvalidBalanceAllocations(totalPercentage);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.CYCLE_CREATE_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.CYCLE_CREATE_ERROR, userMessage, technicalDetails);
                }

                // Validate all balances exist
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

                // Serialize allocations to JSON
                var allocations = request.BalanceAllocations.Select(a => new
                {
                    balanceId = a.BalanceId,
                    percentage = a.Percentage
                }).ToList();

                var balanceAllocationsJson = JsonSerializer.Serialize(allocations);

                // Create cycle entity
                var cycle = new CycleEntity
                {
                    Label = request.Label,
                    Description = request.Description,
                    Cycle = request.Cycle,
                    StartDate = request.StartDate.Date,
                    EndDate = request.EndDate.Date,
                    ApartmentIdsCsv = request.ApartmentIds == null || !request.ApartmentIds.Any()
                        ? null
                        : string.Join(',', request.ApartmentIds),
                    Amount = request.Amount,
                    IsActive = true,
                    BalanceAllocationsJson = balanceAllocationsJson
                };

                // Create cycle in database
                var createdCycle = await _cycleContextManager.CreateAsync(cycle);

                // Generate payments for the cycle (with allocations) using the request
                await GeneratePaymentsForCycle(createdCycle.Id, request);

                _logger.LogInformation("Successfully created cycle {CycleId} with label '{Label}' and {AllocationCount} balance allocations",
                    createdCycle.Id, createdCycle.Label, request.BalanceAllocations.Count);

                // Return cycle with generated payments
                return await _cycleContextManager.GetByIdAsync(createdCycle.Id) ?? createdCycle;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to create cycle", ErrorCodes.CYCLE_CREATE_ERROR);
                throw new ApiException(ErrorCodes.CYCLE_CREATE_ERROR,
                    "Failed to create cycle",
                    $"Label: {request.Label}",
                    ex);
            }
        }

        /// <summary>
        /// Generates payments for all occurrences of the cycle period.
        /// All payments will have balance allocations applied.
        /// Uses PaymentContextManager to maintain proper separation of concerns.
        /// </summary>
        private async Task GeneratePaymentsForCycle(int cycleId, CycleRequest request)
        {
            try
            {
                _logger.LogInformation("Generating payments for cycle {CycleId}", cycleId);

                // Determine apartments for the cycle
                List<int> apartmentIds;
                if (request.ApartmentIds != null && request.ApartmentIds.Any())
                {
                    apartmentIds = request.ApartmentIds;
                }
                else
                {
                    // Get all active apartments using ApartmentContextManager
                    var apartments = await _apartmentContextManager.GetAllAsync(isActive: true);
                    apartmentIds = apartments.Select(a => a.Id).ToList();
                }

                // Calculate payment occurrences based on cycle type
                var occurrences = CalculatePaymentOccurrences(request.Cycle, request.StartDate, request.EndDate);

                _logger.LogInformation("Creating {OccurrenceCount} payment occurrences for {ApartmentCount} apartments", 
                    occurrences.Count, apartmentIds.Count);

                // Create payments for each apartment and occurrence using PaymentContextManager
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

                        // Create payment using context manager
                        var createdPayment = await _paymentContextManager.CreateAsync(payment);

                        // Apply balance allocations to this payment
                        await ApplyBalanceAllocationsToPayment(createdPayment, request.BalanceAllocations);
                    }
                }

                _logger.LogInformation("Successfully created {PaymentCount} payments with allocations for cycle {CycleId}",
                    occurrences.Count * apartmentIds.Count, cycleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate payments for cycle {CycleId}", cycleId);
                throw;
            }
        }

        /// <summary>
        /// Calculates all payment dates based on cycle type and date range
        /// </summary>
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

        /// <summary>
        /// Applies balance allocations to a single payment.
        /// Uses PaymentAllocationContextManager to create allocations.
        /// </summary>
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

                // Save payment allocations using PaymentAllocationContextManager
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
            return await _cycleContextManager.UpdateAsync(id, cycle);
        }

        public async Task<bool> DeleteCycleAsync(int id)
        {
            return await _cycleContextManager.DeleteAsync(id);
        }
    }
}
