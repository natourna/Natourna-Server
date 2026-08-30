using NatournaServer.Constants.Error;
using NatournaServer.Constants.Log;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Models.Api.Requests.Bill;
using NatournaServer.Models.Api.Requests.Paging;
using NatournaServer.Models.Api.Response.Bill;
using NatournaServer.Models.Api.Response.Paging;
using NatournaServer.Models.Entities;

namespace NatournaServer.Services.Api
{
    public class BillApiManager : IBillApiManager
    {
        private readonly IBillContextManager _billContextManager;
        private readonly IBalanceContextManager _balanceContextManager;
        private readonly ITransactionManager _transactionManager;
        private readonly IAuditService _auditService;
        private readonly ILogger<BillApiManager> _logger;

        public BillApiManager(IBillContextManager billContextManager, IBalanceContextManager balanceContextManager, ITransactionManager transactionManager, IAuditService auditService, ILogger<BillApiManager> logger)
        {
            _billContextManager = billContextManager;
            _balanceContextManager = balanceContextManager;
            _transactionManager = transactionManager;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<PagedResponse<BillResponse>> GetPagedBillsAsync(PagedQuery query, int? balanceId = null, bool? isPaid = null)
        {
            var (items, totalCount) = await _billContextManager.GetPagedAsync(query.Page, query.PageSize, balanceId, isPaid);

            return new PagedResponse<BillResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<BillResponse?> GetBillByIdAsync(int id)
        {
            BillEntity? bill = await _billContextManager.GetByIdAsync(id);
            return bill == null ? null : MapToResponse(bill);
        }

        public async Task<BillResponse> CreateBillAsync(BillRequest bill)
        {
            await EnsureBalanceExistsAsync(bill.BalanceId);

            BillEntity billEntity = new(bill.Label, bill.Amount, bill.BalanceId)
            {
                DueDate = bill.DueDate,
                IsPaid = false
            };

            BillEntity created = await _billContextManager.CreateAsync(billEntity);

            await _auditService.LogAsync(LogAction.Create, "Bill", created.Id, null, new { created.Amount, created.BalanceId, created.IsPaid });

            return MapToResponse(created);
        }

        public async Task<BillResponse?> UpdateBillAsync(int id, BillUpdateRequest bill)
        {
            BillEntity? existing = await _billContextManager.GetByIdAsync(id);

            if (existing == null)
            {
                return null;
            }

            BillEntity billEntity = new(bill.Label, bill.Amount, existing.BalanceId)
            {
                DueDate = bill.DueDate,
                IsPaid = bill.IsPaid,
                PaymentDate = bill.PaymentDate
            };

            var oldValues = new
            {
                existing.Amount,
                existing.BalanceId,
                existing.IsPaid
            };

            BillEntity? updated = await _billContextManager.UpdateAsync(id, billEntity);

            if (updated == null)
            {
                return null;
            }

            await _auditService.LogAsync(LogAction.Update, "Bill", id, oldValues, new { updated.Amount, updated.BalanceId, updated.IsPaid });

            return MapToResponse(updated);
        }

        public async Task<bool> DeleteBillAsync(int id)
        {
            BillEntity? existing = await _billContextManager.GetByIdAsync(id);

            if (existing == null)
            {
                return false;
            }

            await _auditService.LogAsync(LogAction.Delete, "Bill", id, new { existing.Amount, existing.BalanceId }, null);

            return await _billContextManager.DeleteAsync(id);
        }

        public Task<BillResponse> MarkBillAsPaidAsync(int billId)
        {
            return SetBillPaidStateAsync(billId, markAsPaid: true);
        }

        public Task<BillResponse> MarkBillAsUnpaidAsync(int billId)
        {
            return SetBillPaidStateAsync(billId, markAsPaid: false);
        }

        /// <summary>Pays or un-pays a bill and moves the money on its balance atomically.</summary>
        private async Task<BillResponse> SetBillPaidStateAsync(int billId, bool markAsPaid)
        {
            string failureCode = markAsPaid ? ErrorCodes.BILL_MARK_AS_PAID_ERROR : ErrorCodes.BILL_MARK_AS_UNPAID_ERROR;

            try
            {
                _logger.LogInformation("Marking bill {BillId} as {State}", billId, markAsPaid ? "paid" : "unpaid");

                BillEntity bill = await GetBillOrThrowAsync(billId);

                EnsureBillIsNotAlreadyInState(bill, markAsPaid);

                BalanceEntity balance = await GetBalanceOrThrowAsync(bill.BalanceId);

                // Paying draws the money out of the balance; un-paying puts it back
                if (markAsPaid && balance.CurrentAmount < bill.Amount)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Bill.InsufficientBalance(billId, balance.Id, bill.Amount, balance.CurrentAmount);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_INSUFFICIENT_BALANCE_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_INSUFFICIENT_BALANCE_ERROR, userMessage, technicalDetails, statusCode: 422);
                }

                BillEntity updatedBill = await _transactionManager.ExecuteInTransactionAsync(async () =>
                {
                    balance.CurrentAmount += markAsPaid ? -bill.Amount : bill.Amount;
                    balance.UpdatedAt = DateTime.UtcNow;
                    await _balanceContextManager.UpdateAsync(balance.Id, balance);

                    bill.IsPaid = markAsPaid;
                    bill.PaymentDate = markAsPaid ? DateTime.UtcNow : null;
                    bill.UpdatedAt = DateTime.UtcNow;

                    BillEntity? result = await _billContextManager.UpdateAsync(billId, bill);

                    if (result == null)
                    {
                        (string userMessage, string technicalDetails) = markAsPaid
                            ? ErrorMessageBuilder.Bill.MarkAsPaidFailed(billId)
                            : ErrorMessageBuilder.Bill.MarkAsUnpaidFailed(billId);
                        throw new ApiException(failureCode, userMessage, technicalDetails, statusCode: 500);
                    }

                    return result;
                });

                await _auditService.LogAsync(LogAction.Update, "Bill", billId,
                    new { IsPaid = !markAsPaid },
                    new { IsPaid = markAsPaid, updatedBill.PaymentDate });

                _logger.LogInformation("Bill {BillId} marked as {State}; balance {BalanceId} adjusted by {Amount:C}",
                    billId, markAsPaid ? "paid" : "unpaid", balance.Id, markAsPaid ? -bill.Amount : bill.Amount);

                return MapToResponse(updatedBill);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = markAsPaid
                    ? ErrorMessageBuilder.Bill.MarkAsPaidFailed(billId)
                    : ErrorMessageBuilder.Bill.MarkAsUnpaidFailed(billId);
                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", failureCode, userMessage);
                throw new ApiException(failureCode, userMessage, technicalDetails, ex, statusCode: 500);
            }
        }

        private async Task<BillEntity> GetBillOrThrowAsync(int billId)
        {
            BillEntity? bill = await _billContextManager.GetByIdAsync(billId);

            if (bill == null)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Bill.BillNotFound(billId);
                _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_NOT_FOUND_ERROR, userMessage);
                throw new ApiException(ErrorCodes.BILL_NOT_FOUND_ERROR, userMessage, technicalDetails, statusCode: 404);
            }

            return bill;
        }

        private void EnsureBillIsNotAlreadyInState(BillEntity bill, bool markAsPaid)
        {
            if (bill.IsPaid != markAsPaid)
            {
                return;
            }

            (string errorCode, (string userMessage, string technicalDetails)) = markAsPaid
                ? (ErrorCodes.BILL_ALREADY_PAID_ERROR, ErrorMessageBuilder.Bill.AlreadyPaid(bill.Id))
                : (ErrorCodes.BILL_ALREADY_UNPAID_ERROR, ErrorMessageBuilder.Bill.AlreadyUnpaid(bill.Id));

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

        private async Task EnsureBalanceExistsAsync(int balanceId)
        {
            await GetBalanceOrThrowAsync(balanceId);
        }

        private static BillResponse MapToResponse(BillEntity bill)
        {
            return new BillResponse
            {
                Id = bill.Id,
                Label = bill.Label,
                Amount = bill.Amount,
                DueDate = bill.DueDate,
                IsPaid = bill.IsPaid,
                PaymentDate = bill.PaymentDate,
                BalanceId = bill.BalanceId,
                BalanceName = bill.Balance?.Label,
                CreatedAt = bill.CreatedAt,
                UpdatedAt = bill.UpdatedAt
            };
        }
    }
}
