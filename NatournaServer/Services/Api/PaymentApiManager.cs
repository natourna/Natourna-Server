using NatournaServer.Constants.Error;
using NatournaServer.Constants.Log;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Models.Api.Requests.Payment;
using NatournaServer.Models.Api.Response.Paging;
using NatournaServer.Models.Api.Response.Payment;
using NatournaServer.Models.Entities;

namespace NatournaServer.Services.Api
{
    public class PaymentApiManager : IPaymentApiManager
    {
        private readonly IPaymentContextManager _paymentContextManager;
        private readonly IPaymentAllocationContextManager _paymentAllocationContextManager;
        private readonly IBalanceContextManager _balanceContextManager;
        private readonly IApartmentContextManager _apartmentContextManager;
        private readonly IAuditService _auditService;
        private readonly ILogger<PaymentApiManager> _logger;

        public PaymentApiManager(IPaymentContextManager paymentContextManager, IPaymentAllocationContextManager paymentAllocationContextManager, IBalanceContextManager balanceContextManager, IApartmentContextManager apartmentContextManager, IAuditService auditService, ILogger<PaymentApiManager> logger)
        {
            _paymentContextManager = paymentContextManager;
            _paymentAllocationContextManager = paymentAllocationContextManager;
            _balanceContextManager = balanceContextManager;
            _apartmentContextManager = apartmentContextManager;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<PagedResponse<PaymentResponse>> GetPaymentsAsync(int page, int pageSize, int? apartmentId, bool? isPaid, DateTime? dueBefore)
        {
            (List<PaymentEntity> items, int totalCount) = await _paymentContextManager.GetPagedAsync(page, pageSize, apartmentId, isPaid, dueBefore);

            return new PagedResponse<PaymentResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PaymentResponse?> GetPaymentByIdAsync(int id)
        {
            PaymentEntity? payment = await _paymentContextManager.GetByIdAsync(id);
            return payment != null ? MapToResponse(payment) : null;
        }

        public async Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request)
        {
            try
            {
                decimal totalPercentage = request.Allocations.Sum(a => a.Percentage);
                if (totalPercentage != 100)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.InvalidBalanceAllocations(totalPercentage);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_INVALID_ALLOCATIONS_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.PAYMENT_INVALID_ALLOCATIONS_ERROR, userMessage, technicalDetails);
                }

                await EnsureApartmentExistsAsync(request.ApartmentId);

                foreach (int balanceId in request.Allocations.Select(x => x.BalanceId))
                {
                    BalanceEntity? balance = await _balanceContextManager.GetByIdAsync(balanceId);

                    if (balance == null)
                    {
                        (string userMessage, string technicalDetails) = ErrorMessageBuilder.Balance.NotFound(balanceId);
                        _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage);
                        throw new ApiException(ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage, technicalDetails);
                    }
                }

                PaymentEntity payment = new(request.Label, request.Amount, request.ApartmentId)
                {
                    DueDate = request.DueDate,
                    CycleId = null
                };

                PaymentEntity createdPayment = await _paymentContextManager.CreateAsync(payment);

                List<PaymentAllocationEntity> paymentAllocations = new();

                foreach (PaymentAllocationRequest allocation in request.Allocations)
                {
                    decimal allocatedAmount = request.Amount * (allocation.Percentage / 100m);

                    PaymentAllocationEntity paymentAllocation = new()
                    {
                        PaymentId = createdPayment.Id,
                        BalanceId = allocation.BalanceId,
                        Percentage = allocation.Percentage,
                        AllocatedAmount = allocatedAmount
                    };

                    paymentAllocations.Add(paymentAllocation);
                }

                await _paymentAllocationContextManager.CreateRangeAsync(paymentAllocations);

                await _auditService.LogAsync(LogAction.Create, "Payment", createdPayment.Id, null, new { createdPayment.Amount, createdPayment.ApartmentId, createdPayment.IsPaid, AllocationCount = request.Allocations.Count });

                _logger.LogInformation("Created payment {PaymentId} with {AllocationCount} balance allocations", createdPayment.Id, request.Allocations.Count);

                PaymentEntity? reloaded = await _paymentContextManager.GetByIdAsync(createdPayment.Id);

                return MapToResponse(reloaded ?? createdPayment);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.CreateWithBalancesFailed(request);
                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_CREATE_ERROR, userMessage);
                throw new ApiException(ErrorCodes.PAYMENT_CREATE_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<PaymentResponse?> UpdatePaymentAsync(int id, PaymentUpdateRequest request)
        {
            PaymentEntity? existing = await _paymentContextManager.GetByIdAsync(id);

            if (existing == null)
            {
                return null;
            }

            await EnsureApartmentExistsAsync(request.ApartmentId);

            var oldValues = new
            {
                existing.Amount,
                existing.ApartmentId,
                existing.IsPaid,
                existing.DueDate
            };

            PaymentEntity? updated = await _paymentContextManager.UpdateAsync(id, request.Label, request.Amount, request.DueDate, request.ApartmentId);

            if (updated != null)
            {
                await _auditService.LogAsync(LogAction.Update, "Payment", id, oldValues, new { updated.Amount, updated.ApartmentId, updated.IsPaid, updated.DueDate });

                return MapToResponse(updated);
            }

            return null;
        }

        public async Task<bool> DeletePaymentAsync(int id)
        {
            PaymentEntity? existing = await _paymentContextManager.GetByIdAsync(id);

            if (existing == null)
            {
                return false;
            }

            await _auditService.LogAsync(LogAction.Delete, "Payment", id, new { existing.Amount, existing.ApartmentId, existing.IsPaid }, null);

            return await _paymentContextManager.DeleteAsync(id);
        }

        public async Task<PaymentResponse> MarkPaymentAsPaidAsync(int paymentId)
        {
            try
            {
                PaymentEntity? payment = await _paymentContextManager.GetByIdAsync(paymentId);

                if (payment == null)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.PaymentNotFound(paymentId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_NOT_FOUND_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.PAYMENT_NOT_FOUND_ERROR, userMessage, technicalDetails);
                }

                if (payment.IsPaid)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.AlreadyPaid(paymentId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_ALREADY_PAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.PAYMENT_ALREADY_PAID_ERROR, userMessage, technicalDetails);
                }

                List<PaymentAllocationEntity> allocations = await _paymentAllocationContextManager.GetAllAsync(paymentId: paymentId);
                if (allocations.Count == 0)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.MarkAsPaidFailed(paymentId);
                    _logger.LogWarning("[{ErrorCode}] No allocations found for payment {PaymentId}", ErrorCodes.PAYMENT_MARK_AS_PAID_ERROR, paymentId);
                    throw new ApiException(ErrorCodes.PAYMENT_MARK_AS_PAID_ERROR, userMessage, $"{technicalDetails}, No allocations found");
                }

                List<(int balanceId, decimal amount)> balanceUpdates = [];

                foreach (PaymentAllocationEntity allocation in allocations)
                {
                    BalanceEntity? balance = await _balanceContextManager.GetByIdAsync(allocation.BalanceId);

                    if (balance == null)
                    {
                        await RollbackBalanceUpdates(balanceUpdates, subtract: true);

                        (string userMessage, string technicalDetails) = ErrorMessageBuilder.Balance.NotFound(allocation.BalanceId);
                        _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage);
                        throw new ApiException(ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage, technicalDetails);
                    }

                    balance.CurrentAmount += allocation.AllocatedAmount;
                    balance.UpdatedAt = DateTime.UtcNow;

                    BalanceEntity? updatedBalance = await _balanceContextManager.UpdateAsync(balance.Id, balance);

                    if (updatedBalance == null)
                    {
                        await RollbackBalanceUpdates(balanceUpdates, subtract: true);

                        (string userMessage, string technicalDetails) = ErrorMessageBuilder.Balance.UpdateFailed(balance.Id, balance);
                        _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_UPDATE_ERROR, userMessage);
                        throw new ApiException(ErrorCodes.BALANCE_UPDATE_ERROR, userMessage, technicalDetails);
                    }

                    balanceUpdates.Add((balance.Id, allocation.AllocatedAmount));
                }

                DateTime paymentDate = DateTime.UtcNow;
                PaymentEntity? updatedPayment = await _paymentContextManager.SetPaidStatusAsync(paymentId, true, paymentDate);

                if (updatedPayment == null)
                {
                    await RollbackBalanceUpdates(balanceUpdates, subtract: true);

                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.MarkAsPaidFailed(paymentId);
                    _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_MARK_AS_PAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.PAYMENT_MARK_AS_PAID_ERROR, userMessage, technicalDetails);
                }

                await _auditService.LogAsync(LogAction.Update, "Payment", paymentId, new { IsPaid = false, PaymentDate = (DateTime?)null }, new { IsPaid = true, PaymentDate = paymentDate });

                _logger.LogInformation("Marked payment {PaymentId} as paid and credited {BalanceCount} balances", paymentId, allocations.Count);

                return MapToResponse(updatedPayment);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.MarkAsPaidFailed(paymentId);
                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_MARK_AS_PAID_ERROR, userMessage);
                throw new ApiException(ErrorCodes.PAYMENT_MARK_AS_PAID_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<PaymentResponse> MarkPaymentAsUnpaidAsync(int paymentId)
        {
            try
            {
                PaymentEntity? payment = await _paymentContextManager.GetByIdAsync(paymentId);

                if (payment == null)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.PaymentNotFound(paymentId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_NOT_FOUND_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.PAYMENT_NOT_FOUND_ERROR, userMessage, technicalDetails);
                }

                if (!payment.IsPaid)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.AlreadyUnpaid(paymentId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_ALREADY_UNPAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.PAYMENT_ALREADY_UNPAID_ERROR, userMessage, technicalDetails);
                }

                List<PaymentAllocationEntity> allocations = await _paymentAllocationContextManager.GetAllAsync(paymentId: paymentId);

                if (allocations.Count == 0)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.MarkAsUnpaidFailed(paymentId);
                    _logger.LogWarning("[{ErrorCode}] No allocations found for payment {PaymentId}", ErrorCodes.PAYMENT_MARK_AS_UNPAID_ERROR, paymentId);
                    throw new ApiException(ErrorCodes.PAYMENT_MARK_AS_UNPAID_ERROR, userMessage, $"{technicalDetails}, No allocations found");
                }

                List<(int balanceId, decimal amount)> balanceUpdates = new();

                foreach (PaymentAllocationEntity allocation in allocations)
                {
                    BalanceEntity? balance = await _balanceContextManager.GetByIdAsync(allocation.BalanceId);

                    if (balance == null)
                    {
                        await RollbackBalanceUpdates(balanceUpdates, subtract: false);

                        (string userMessage, string technicalDetails) = ErrorMessageBuilder.Balance.NotFound(allocation.BalanceId);
                        _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage);
                        throw new ApiException(ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage, technicalDetails);
                    }

                    balance.CurrentAmount -= allocation.AllocatedAmount;
                    balance.UpdatedAt = DateTime.UtcNow;

                    BalanceEntity? updatedBalance = await _balanceContextManager.UpdateAsync(balance.Id, balance);

                    if (updatedBalance == null)
                    {
                        await RollbackBalanceUpdates(balanceUpdates, subtract: false);

                        (string userMessage, string technicalDetails) = ErrorMessageBuilder.Balance.UpdateFailed(balance.Id, balance);
                        _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_UPDATE_ERROR, userMessage);
                        throw new ApiException(ErrorCodes.BALANCE_UPDATE_ERROR, userMessage, technicalDetails);
                    }

                    balanceUpdates.Add((balance.Id, allocation.AllocatedAmount));
                }

                PaymentEntity? updatedPayment = await _paymentContextManager.SetPaidStatusAsync(paymentId, false, null);

                if (updatedPayment == null)
                {
                    await RollbackBalanceUpdates(balanceUpdates, subtract: false);

                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.MarkAsUnpaidFailed(paymentId);
                    _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_MARK_AS_UNPAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.PAYMENT_MARK_AS_UNPAID_ERROR, userMessage, technicalDetails);
                }

                await _auditService.LogAsync(LogAction.Update, "Payment", paymentId, new { IsPaid = true, payment.PaymentDate }, new { IsPaid = false, PaymentDate = (DateTime?)null });

                _logger.LogInformation("Marked payment {PaymentId} as unpaid and debited {BalanceCount} balances", paymentId, allocations.Count);

                return MapToResponse(updatedPayment);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.MarkAsUnpaidFailed(paymentId);
                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_MARK_AS_UNPAID_ERROR, userMessage);
                throw new ApiException(ErrorCodes.PAYMENT_MARK_AS_UNPAID_ERROR, userMessage, technicalDetails, ex);
            }
        }

        private async Task EnsureApartmentExistsAsync(int apartmentId)
        {
            var apartment = await _apartmentContextManager.GetByIdAsync(apartmentId);
            if (apartment == null)
            {
                throw new ApiException(ErrorCodes.PAYMENT_APARTMENT_INVALID_ERROR, "The requested apartment does not exist", $"ApartmentId: {apartmentId}");
            }
        }

        private async Task RollbackBalanceUpdates(List<(int balanceId, decimal amount)> updates, bool subtract)
        {
            foreach ((int balanceId, decimal amount) in updates)
            {
                try
                {
                    BalanceEntity? balance = await _balanceContextManager.GetByIdAsync(balanceId);

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

                        balance.UpdatedAt = DateTime.UtcNow;
                        await _balanceContextManager.UpdateAsync(balance.Id, balance);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rollback balance {BalanceId}", balanceId);
                }
            }
        }

        private static PaymentResponse MapToResponse(PaymentEntity payment)
        {
            return new PaymentResponse
            {
                Id = payment.Id,
                Label = payment.Label,
                Amount = payment.Amount,
                DueDate = payment.DueDate,
                IsPaid = payment.IsPaid,
                ApartmentId = payment.ApartmentId,
                ApartmentInfo = payment.Apartment?.ApartmentInfo,
                ApartmentOwner = payment.Apartment?.Owner,
                ApartmentTenant = payment.Apartment?.Tenant,
                CycleId = payment.CycleId,
                CycleName = payment.Cycle?.Label,
                Recurrent = payment.Recurrent,
                PaymentDate = payment.PaymentDate,
                PaymentOccurrence = payment.Cycle?.Cycle.ToString(),
                Allocations = payment.PaymentAllocations.Select(MapAllocationsToResponse).ToList(),
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };
        }

        private static PaymentAllocationResponse MapAllocationsToResponse(PaymentAllocationEntity paymentAllocation)
        {
            return new PaymentAllocationResponse
            {
                Id = paymentAllocation.Id,
                PaymentId = paymentAllocation.PaymentId,
                BalanceId = paymentAllocation.BalanceId,
                BalanceName = paymentAllocation.Balance?.Label,
                Percentage = paymentAllocation.Percentage,
                AllocatedAmount = paymentAllocation.AllocatedAmount,
                CreatedAt = paymentAllocation.CreatedAt,
                UpdatedAt = paymentAllocation.UpdatedAt
            };
        }
    }
}
