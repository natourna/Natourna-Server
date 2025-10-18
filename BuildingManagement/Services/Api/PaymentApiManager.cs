using BuildingManagement.Constants;
using BuildingManagement.Exceptions;
using BuildingManagement.Interfaces.Api;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Models.Entities;
using BuildingManagement.Models.Requests.Payment;

namespace BuildingManagement.Services.Api
{
    public class PaymentApiManager : IPaymentApiManager
    {
        private readonly IPaymentContextManager _paymentContextManager;
        private readonly IPaymentAllocationContextManager _paymentAllocationContextManager;
        private readonly IBalanceContextManager _balanceContextManager;
        private readonly ILogger<PaymentApiManager> _logger;

        public PaymentApiManager(IPaymentContextManager paymentContextManager, IPaymentAllocationContextManager paymentAllocationContextManager, IBalanceContextManager balanceContextManager, ILogger<PaymentApiManager> logger)
        {
            _paymentContextManager = paymentContextManager;
            _paymentAllocationContextManager = paymentAllocationContextManager;
            _balanceContextManager = balanceContextManager;
            _logger = logger;
        }

        public async Task<List<PaymentEntity>> GetAllPaymentsAsync()
        {
            return await _paymentContextManager.GetAllAsync();
        }

        public async Task<PaymentEntity?> GetPaymentByIdAsync(int id)
        {
            var payments = await _paymentContextManager.GetAllAsync(paymentId: id);
            return payments.FirstOrDefault();
        }

        public async Task<List<PaymentEntity>> GetPaymentsByApartmentIdAsync(int apartmentId)
        {
            return await _paymentContextManager.GetAllAsync(apartmentId: apartmentId);
        }

        public async Task<List<PaymentEntity>> GetPaymentsByCycleIdAsync(int cycleId)
        {
            return await _paymentContextManager.GetAllAsync(cycleId: cycleId);
        }

        public async Task<PaymentEntity> CreatePaymentAsync(PaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Creating payment with balance allocations - Amount: {Amount}, ApartmentId: {ApartmentId}", request.Amount, request.ApartmentId);

                // Validate balance allocations sum to 100% (already validated by attribute, but double-check)
                var totalPercentage = request.Allocations.Sum(a => a.Percentage);
                if (totalPercentage != 100)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Payment.InvalidBalanceAllocations(totalPercentage);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_INVALID_ALLOCATIONS_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.PAYMENT_INVALID_ALLOCATIONS_ERROR, userMessage, technicalDetails);
                }

                // Validate all balances exist
                foreach (var allocation in request.Allocations)
                {
                    var balances = await _balanceContextManager.GetAllAsync(balanceId: allocation.BalanceId);
                    if (!balances.Any())
                    {
                        var (userMessage, technicalDetails) = ErrorMessageBuilder.Balance.NotFound(allocation.BalanceId);
                        _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage);
                        throw new ApiException(ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage, technicalDetails);
                    }
                }

                // Create the payment (standalone, not part of a cycle)
                var payment = new PaymentEntity(request.Amount, request.ApartmentId)
                {
                    DueDate = request.DueDate,
                    IsPaid = request.IsPaid,
                    PaymentDate = request.IsPaid ? (request.PaymentDate ?? DateTime.UtcNow) : null,
                    CycleId = null
                };

                var createdPayment = await _paymentContextManager.CreateAsync(payment);

                // Create balance allocations
                var paymentAllocations = new List<PaymentAllocationEntity>();
                foreach (var allocation in request.Allocations)
                {
                    var allocatedAmount = request.Amount * (allocation.Percentage / 100m);

                    var paymentAllocation = new PaymentAllocationEntity
                    {
                        PaymentId = createdPayment.Id,
                        BalanceId = allocation.BalanceId,
                        Percentage = allocation.Percentage,
                        AllocatedAmount = allocatedAmount
                    };

                    paymentAllocations.Add(paymentAllocation);
                }

                await _paymentAllocationContextManager.CreateRangeAsync(paymentAllocations);

                _logger.LogInformation("Successfully saved {AllocationCount} payment allocations", paymentAllocations.Count);

                await UpdateBalancesForPayment(paymentAllocations);

                _logger.LogInformation("Successfully created payment {PaymentId} with {AllocationCount} balance allocations", createdPayment.Id, request.Allocations.Count);

                return createdPayment;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var (userMessage, technicalDetails) = ErrorMessageBuilder.Payment.CreateWithBalancesFailed(request);
                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_CREATE_ERROR, userMessage);
                throw new ApiException(ErrorCodes.PAYMENT_CREATE_ERROR, userMessage, technicalDetails, ex);
            }
        }

        private async Task UpdateBalancesForPayment(List<PaymentAllocationEntity> paymentAllocations)
        {
            foreach (var paymentAllocation in paymentAllocations)
            {
                var balances = await _balanceContextManager.GetAllAsync(balanceId: paymentAllocation.BalanceId);
                var balance = balances.FirstOrDefault();

                if (balance != null)
                {
                    balance.CurrentAmount += paymentAllocation.AllocatedAmount;
                    balance.UpdatededAt = DateTime.UtcNow;
                    await _balanceContextManager.UpdateAsync(balance.Id, balance);

                    _logger.LogInformation("Added {Amount:C} to balance {BalanceId} (PaymentAllocation {PaymentAllocationId})", paymentAllocation.AllocatedAmount, balance.Id, paymentAllocation.Id);
                }
            }
        }

        public async Task<PaymentEntity?> UpdatePaymentAsync(int id, PaymentEntity payment)
        {
            return await _paymentContextManager.UpdateAsync(id, payment);
        }

        public async Task<bool> DeletePaymentAsync(int id)
        {
            return await _paymentContextManager.DeleteAsync(id);
        }

        public async Task<PaymentEntity> MarkPaymentAsPaidAsync(int paymentId)
        {
            try
            {
                _logger.LogInformation("Marking payment {PaymentId} as paid", paymentId);

                // Get the payment
                var payment = await GetPaymentByIdAsync(paymentId);

                if (payment == null)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Payment.PaymentNotFound(paymentId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_NOT_FOUND_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.PAYMENT_NOT_FOUND_ERROR, userMessage, technicalDetails);
                }

                // Check if already paid
                if (payment.IsPaid == true)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Payment.AlreadyPaid(paymentId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_ALREADY_PAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.PAYMENT_ALREADY_PAID_ERROR, userMessage, technicalDetails);
                }

                // Get payment allocations
                var allocations = await _paymentAllocationContextManager.GetAllAsync(paymentId: paymentId);
                if (!allocations.Any())
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Payment.MarkAsPaidFailed(paymentId);
                    _logger.LogWarning("[{ErrorCode}] No allocations found for payment {PaymentId}", ErrorCodes.PAYMENT_MARK_AS_PAID_ERROR, paymentId);
                    throw new ApiException(ErrorCodes.PAYMENT_MARK_AS_PAID_ERROR, userMessage, $"{technicalDetails}, No allocations found");
                }

                // Add allocated amounts to balances
                var balanceUpdates = new List<(int balanceId, decimal amount)>();
                foreach (var allocation in allocations)
                {
                    var balances = await _balanceContextManager.GetAllAsync(balanceId: allocation.BalanceId);
                    var balance = balances.FirstOrDefault();

                    if (balance == null)
                    {
                        var (userMessage, technicalDetails) = ErrorMessageBuilder.Balance.NotFound(allocation.BalanceId);
                        _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage);
                        throw new ApiException(ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage, technicalDetails);
                    }

                    // Add amount to balance (positive operation)
                    balance.CurrentAmount += allocation.AllocatedAmount;
                    balance.UpdatededAt = DateTime.UtcNow;
                    var updatedBalance = await _balanceContextManager.UpdateAsync(balance.Id, balance);

                    if (updatedBalance == null)
                    {
                        // Rollback previous balance updates
                        await RollbackBalanceUpdates(balanceUpdates, subtract: true);

                        var (userMessage, technicalDetails) = ErrorMessageBuilder.Balance.UpdateFailed(balance.Id, balance);
                        _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_UPDATE_ERROR, userMessage);
                        throw new ApiException(ErrorCodes.BALANCE_UPDATE_ERROR, userMessage, technicalDetails);
                    }

                    balanceUpdates.Add((balance.Id, allocation.AllocatedAmount));

                    _logger.LogInformation("Added {Amount:C} to balance {BalanceId} (Payment {PaymentId})",
                        allocation.AllocatedAmount, balance.Id, paymentId);
                }

                // Mark payment as paid with payment date
                payment.IsPaid = true;
                payment.PaymentDate = DateTime.UtcNow;
                payment.UpdatededAt = DateTime.UtcNow;
                var updatedPayment = await _paymentContextManager.UpdateAsync(paymentId, payment);

                if (updatedPayment == null)
                {
                    // Rollback: Remove amounts from balances
                    await RollbackBalanceUpdates(balanceUpdates, subtract: true);

                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Payment.MarkAsPaidFailed(paymentId);
                    _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_MARK_AS_PAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.PAYMENT_MARK_AS_PAID_ERROR, userMessage, technicalDetails);
                }

                _logger.LogInformation("Successfully marked payment {PaymentId} as paid on {PaymentDate} and added funds to {BalanceCount} balances",
                    paymentId, payment.PaymentDate, allocations.Count);

                return updatedPayment;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var (userMessage, technicalDetails) = ErrorMessageBuilder.Payment.MarkAsPaidFailed(paymentId);
                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_MARK_AS_PAID_ERROR, userMessage);
                throw new ApiException(ErrorCodes.PAYMENT_MARK_AS_PAID_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<PaymentEntity> MarkPaymentAsUnpaidAsync(int paymentId)
        {
            try
            {
                _logger.LogInformation("Marking payment {PaymentId} as unpaid", paymentId);

                var payment = await GetPaymentByIdAsync(paymentId);
                if (payment == null)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Payment.PaymentNotFound(paymentId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_NOT_FOUND_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.PAYMENT_NOT_FOUND_ERROR, userMessage, technicalDetails);
                }

                // Check if already unpaid
                if (payment.IsPaid == false)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Payment.AlreadyUnpaid(paymentId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_ALREADY_UNPAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.PAYMENT_ALREADY_UNPAID_ERROR, userMessage, technicalDetails);
                }

                // Get payment allocations
                var allocations = await _paymentAllocationContextManager.GetAllAsync(paymentId: paymentId);
                if (!allocations.Any())
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Payment.MarkAsUnpaidFailed(paymentId);
                    _logger.LogWarning("[{ErrorCode}] No allocations found for payment {PaymentId}", ErrorCodes.PAYMENT_MARK_AS_UNPAID_ERROR, paymentId);
                    throw new ApiException(ErrorCodes.PAYMENT_MARK_AS_UNPAID_ERROR, userMessage, $"{technicalDetails}, No allocations found");
                }

                // Deduct allocated amounts from balances
                var balanceUpdates = new List<(int balanceId, decimal amount)>();
                foreach (var allocation in allocations)
                {
                    var balances = await _balanceContextManager.GetAllAsync(balanceId: allocation.BalanceId);
                    var balance = balances.FirstOrDefault();

                    if (balance == null)
                    {
                        var (userMessage, technicalDetails) = ErrorMessageBuilder.Balance.NotFound(allocation.BalanceId);
                        _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage);
                        throw new ApiException(ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage, technicalDetails);
                    }

                    // Deduct amount from balance (reverse the payment)
                    balance.CurrentAmount -= allocation.AllocatedAmount;
                    balance.UpdatededAt = DateTime.UtcNow;
                    var updatedBalance = await _balanceContextManager.UpdateAsync(balance.Id, balance);

                    if (updatedBalance == null)
                    {
                        // Rollback previous balance updates
                        await RollbackBalanceUpdates(balanceUpdates, subtract: false);

                        var (userMessage, technicalDetails) = ErrorMessageBuilder.Balance.UpdateFailed(balance.Id, balance);
                        _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_UPDATE_ERROR, userMessage);
                        throw new ApiException(ErrorCodes.BALANCE_UPDATE_ERROR, userMessage, technicalDetails);
                    }

                    balanceUpdates.Add((balance.Id, allocation.AllocatedAmount));

                    _logger.LogInformation("Deducted {Amount:C} from balance {BalanceId} (Payment {PaymentId})",
                        allocation.AllocatedAmount, balance.Id, paymentId);
                }

                // Mark payment as unpaid and clear payment date
                payment.IsPaid = false;
                payment.PaymentDate = null;
                payment.UpdatededAt = DateTime.UtcNow;
                var updatedPayment = await _paymentContextManager.UpdateAsync(paymentId, payment);

                if (updatedPayment == null)
                {
                    // Rollback: Add amounts back to balances
                    await RollbackBalanceUpdates(balanceUpdates, subtract: false);

                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Payment.MarkAsUnpaidFailed(paymentId);
                    _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_MARK_AS_UNPAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.PAYMENT_MARK_AS_UNPAID_ERROR, userMessage, technicalDetails);
                }

                _logger.LogInformation("Successfully marked payment {PaymentId} as unpaid and deducted funds from {BalanceCount} balances",
                    paymentId, allocations.Count);

                return updatedPayment;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var (userMessage, technicalDetails) = ErrorMessageBuilder.Payment.MarkAsUnpaidFailed(paymentId);
                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_MARK_AS_UNPAID_ERROR, userMessage);
                throw new ApiException(ErrorCodes.PAYMENT_MARK_AS_UNPAID_ERROR, userMessage, technicalDetails, ex);
            }
        }

        private async Task RollbackBalanceUpdates(List<(int balanceId, decimal amount)> updates, bool subtract)
        {
            foreach (var (balanceId, amount) in updates)
            {
                try
                {
                    var balances = await _balanceContextManager.GetAllAsync(balanceId: balanceId);
                    var balance = balances.FirstOrDefault();

                    if (balance != null)
                    {
                        if (subtract)
                        {
                            balance.CurrentAmount -= amount;
                        }
                        else
                        {
                            balance.CurrentAmount += amount;
                        }

                        balance.UpdatededAt = DateTime.UtcNow;
                        await _balanceContextManager.UpdateAsync(balance.Id, balance);

                        _logger.LogInformation("Rolled back balance {BalanceId} by {Amount:C}", balanceId, amount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rollback balance {BalanceId}", balanceId);
                }
            }
        }
    }
}
