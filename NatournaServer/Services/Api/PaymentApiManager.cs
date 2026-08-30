using NatournaServer.Constants.Error;
using NatournaServer.Constants.Log;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Models.Api.Requests.Paging;
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
        private readonly ICycleContextManager _cycleContextManager;
        private readonly ITransactionManager _transactionManager;
        private readonly IAuditService _auditService;
        private readonly ILogger<PaymentApiManager> _logger;

        public PaymentApiManager(
            IPaymentContextManager paymentContextManager,
            IPaymentAllocationContextManager paymentAllocationContextManager,
            IBalanceContextManager balanceContextManager,
            IApartmentContextManager apartmentContextManager,
            ICycleContextManager cycleContextManager,
            ITransactionManager transactionManager,
            IAuditService auditService,
            ILogger<PaymentApiManager> logger)
        {
            _paymentContextManager = paymentContextManager;
            _paymentAllocationContextManager = paymentAllocationContextManager;
            _balanceContextManager = balanceContextManager;
            _apartmentContextManager = apartmentContextManager;
            _cycleContextManager = cycleContextManager;
            _transactionManager = transactionManager;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<PagedResponse<PaymentResponse>> GetPagedPaymentsAsync(PagedQuery query, int? apartmentId = null, int? cycleId = null, bool? isPaid = null)
        {
            var (items, totalCount) = await _paymentContextManager.GetPagedAsync(query.Page, query.PageSize, apartmentId, cycleId, isPaid);

            return new PagedResponse<PaymentResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                Page = query.Page,
                PageSize = query.PageSize,
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
                _logger.LogInformation("Creating payment with balance allocations - Amount: {Amount}, ApartmentId: {ApartmentId}", request.Amount, request.ApartmentId);

                await EnsureApartmentExistsAsync(request.ApartmentId);
                EnsureAllocationsSumTo100(request.Allocations, ErrorCodes.PAYMENT_INVALID_ALLOCATIONS_ERROR);
                await EnsureBalancesExistAsync(request.Allocations);

                PaymentEntity payment = new(request.Label, request.Amount, request.ApartmentId)
                {
                    DueDate = request.DueDate,
                    CycleId = null
                };

                PaymentEntity createdPayment = await _transactionManager.ExecuteInTransactionAsync(async () =>
                {
                    PaymentEntity created = await _paymentContextManager.CreateAsync(payment);
                    await _paymentAllocationContextManager.CreateRangeAsync(BuildAllocations(created, request.Allocations));
                    return created;
                });

                await _auditService.LogAsync(LogAction.Create, "Payment", createdPayment.Id, null, new { createdPayment.Amount, createdPayment.ApartmentId, createdPayment.IsPaid, AllocationCount = request.Allocations.Count });

                _logger.LogInformation("Successfully created payment {PaymentId} with {AllocationCount} balance allocations", createdPayment.Id, request.Allocations.Count);

                return MapToResponse(createdPayment);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.CreateWithBalancesFailed(request);
                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_CREATE_ERROR, userMessage);
                throw new ApiException(ErrorCodes.PAYMENT_CREATE_ERROR, userMessage, technicalDetails, ex, statusCode: 500);
            }
        }

        public async Task<PaymentResponse?> UpdatePaymentAsync(int id, PaymentUpdateRequest payment)
        {
            PaymentEntity? existing = await _paymentContextManager.GetByIdAsync(id);

            if (existing == null)
            {
                return null;
            }

            await EnsureApartmentExistsAsync(payment.ApartmentId);
            await EnsureCycleExistsAsync(payment.CycleId);

            PaymentEntity paymentEntity = new(existing.Label, payment.Amount, payment.ApartmentId)
            {
                PaymentDate = payment.PaymentDate,
                DueDate = payment.DueDate,
                IsPaid = payment.IsPaid,
                CycleId = payment.CycleId
            };

            var oldValues = new
            {
                existing.Amount,
                existing.ApartmentId,
                existing.IsPaid,
                existing.DueDate
            };

            PaymentEntity? updated = await _paymentContextManager.UpdateAsync(id, paymentEntity);

            if (updated == null)
            {
                return null;
            }

            await _auditService.LogAsync(LogAction.Update, "Payment", id, oldValues, new { updated.Amount, updated.ApartmentId, updated.IsPaid, updated.DueDate });

            return MapToResponse(updated);
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

        public Task<PaymentResponse> MarkPaymentAsPaidAsync(int paymentId)
        {
            return SetPaymentPaidStateAsync(paymentId, markAsPaid: true);
        }

        public Task<PaymentResponse> MarkPaymentAsUnpaidAsync(int paymentId)
        {
            return SetPaymentPaidStateAsync(paymentId, markAsPaid: false);
        }

        /// <summary>Pays or un-pays a payment and moves its allocated amounts across the balances atomically.</summary>
        private async Task<PaymentResponse> SetPaymentPaidStateAsync(int paymentId, bool markAsPaid)
        {
            string failureCode = markAsPaid ? ErrorCodes.PAYMENT_MARK_AS_PAID_ERROR : ErrorCodes.PAYMENT_MARK_AS_UNPAID_ERROR;

            try
            {
                _logger.LogInformation("Marking payment {PaymentId} as {State}", paymentId, markAsPaid ? "paid" : "unpaid");

                PaymentEntity payment = await GetPaymentOrThrowAsync(paymentId);

                EnsurePaymentIsNotAlreadyInState(payment, markAsPaid);

                List<PaymentAllocationEntity> allocations = await _paymentAllocationContextManager.GetAllAsync(paymentId: paymentId);

                if (allocations.Count == 0)
                {
                    (string userMessage, string technicalDetails) = markAsPaid
                        ? ErrorMessageBuilder.Payment.MarkAsPaidFailed(paymentId)
                        : ErrorMessageBuilder.Payment.MarkAsUnpaidFailed(paymentId);
                    _logger.LogWarning("[{ErrorCode}] No allocations found for payment {PaymentId}", failureCode, paymentId);
                    throw new ApiException(failureCode, userMessage, $"{technicalDetails}, No allocations found", statusCode: 500);
                }

                PaymentEntity updatedPayment = await _transactionManager.ExecuteInTransactionAsync(async () =>
                {
                    // Paying credits each allocated balance; un-paying debits it back
                    foreach (PaymentAllocationEntity allocation in allocations)
                    {
                        BalanceEntity balance = await GetBalanceOrThrowAsync(allocation.BalanceId);

                        balance.CurrentAmount += markAsPaid ? allocation.AllocatedAmount : -allocation.AllocatedAmount;
                        balance.UpdatedAt = DateTime.UtcNow;
                        await _balanceContextManager.UpdateAsync(balance.Id, balance);
                    }

                    payment.IsPaid = markAsPaid;
                    payment.PaymentDate = markAsPaid ? DateTime.UtcNow : null;
                    payment.UpdatedAt = DateTime.UtcNow;

                    PaymentEntity? result = await _paymentContextManager.UpdateAsync(paymentId, payment);

                    if (result == null)
                    {
                        (string userMessage, string technicalDetails) = markAsPaid
                            ? ErrorMessageBuilder.Payment.MarkAsPaidFailed(paymentId)
                            : ErrorMessageBuilder.Payment.MarkAsUnpaidFailed(paymentId);
                        throw new ApiException(failureCode, userMessage, technicalDetails, statusCode: 500);
                    }

                    return result;
                });

                await _auditService.LogAsync(LogAction.Update, "Payment", paymentId,
                    new { IsPaid = !markAsPaid },
                    new { IsPaid = markAsPaid, updatedPayment.PaymentDate });

                _logger.LogInformation("Payment {PaymentId} marked as {State} across {BalanceCount} balances",
                    paymentId, markAsPaid ? "paid" : "unpaid", allocations.Count);

                return MapToResponse(updatedPayment);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = markAsPaid
                    ? ErrorMessageBuilder.Payment.MarkAsPaidFailed(paymentId)
                    : ErrorMessageBuilder.Payment.MarkAsUnpaidFailed(paymentId);
                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", failureCode, userMessage);
                throw new ApiException(failureCode, userMessage, technicalDetails, ex, statusCode: 500);
            }
        }

        private async Task<PaymentEntity> GetPaymentOrThrowAsync(int paymentId)
        {
            PaymentEntity? payment = await _paymentContextManager.GetByIdAsync(paymentId);

            if (payment == null)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.PaymentNotFound(paymentId);
                _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_NOT_FOUND_ERROR, userMessage);
                throw new ApiException(ErrorCodes.PAYMENT_NOT_FOUND_ERROR, userMessage, technicalDetails, statusCode: 404);
            }

            return payment;
        }

        private void EnsurePaymentIsNotAlreadyInState(PaymentEntity payment, bool markAsPaid)
        {
            if (payment.IsPaid != markAsPaid)
            {
                return;
            }

            (string errorCode, (string userMessage, string technicalDetails)) = markAsPaid
                ? (ErrorCodes.PAYMENT_ALREADY_PAID_ERROR, ErrorMessageBuilder.Payment.AlreadyPaid(payment.Id))
                : (ErrorCodes.PAYMENT_ALREADY_UNPAID_ERROR, ErrorMessageBuilder.Payment.AlreadyUnpaid(payment.Id));

            _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", errorCode, userMessage);
            throw new ApiException(errorCode, userMessage, technicalDetails, statusCode: 409);
        }

        private async Task<BalanceEntity> GetBalanceOrThrowAsync(int balanceId)
        {
            BalanceEntity? balance = await _balanceContextManager.GetByIdAsync(balanceId);

            if (balance == null)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Balance.NotFound(balanceId);
                _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage);
                throw new ApiException(ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage, technicalDetails, statusCode: 404);
            }

            return balance;
        }

        private async Task EnsureApartmentExistsAsync(int apartmentId)
        {
            ApartmentEntity? apartment = await _apartmentContextManager.GetByIdAsync(apartmentId);

            if (apartment == null)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Reference.NotFound("Apartment", apartmentId);
                _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.APARTMENT_NOT_FOUND_ERROR, userMessage);
                throw new ApiException(ErrorCodes.APARTMENT_NOT_FOUND_ERROR, userMessage, technicalDetails, statusCode: 404);
            }
        }

        private async Task EnsureCycleExistsAsync(int? cycleId)
        {
            if (cycleId == null)
            {
                return;
            }

            CycleEntity? cycle = await _cycleContextManager.GetByIdAsync(cycleId.Value);

            if (cycle == null)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Reference.NotFound("Cycle", cycleId.Value);
                _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.CYCLE_NOT_FOUND_ERROR, userMessage);
                throw new ApiException(ErrorCodes.CYCLE_NOT_FOUND_ERROR, userMessage, technicalDetails, statusCode: 404);
            }
        }

        private void EnsureAllocationsSumTo100(List<PaymentAllocationRequest> allocations, string errorCode)
        {
            decimal totalPercentage = allocations.Sum(a => a.Percentage);

            if (totalPercentage != 100)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.InvalidBalanceAllocations(totalPercentage);
                _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", errorCode, userMessage);
                throw new ApiException(errorCode, userMessage, technicalDetails, statusCode: 422);
            }
        }

        private async Task EnsureBalancesExistAsync(List<PaymentAllocationRequest> allocations)
        {
            foreach (int balanceId in allocations.Select(a => a.BalanceId).Distinct())
            {
                await GetBalanceOrThrowAsync(balanceId);
            }
        }

        private static List<PaymentAllocationEntity> BuildAllocations(PaymentEntity payment, List<PaymentAllocationRequest> allocations)
        {
            return allocations.Select(allocation => new PaymentAllocationEntity
            {
                PaymentId = payment.Id,
                BalanceId = allocation.BalanceId,
                Percentage = allocation.Percentage,
                AllocatedAmount = payment.Amount * (allocation.Percentage / 100m)
            }).ToList();
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
