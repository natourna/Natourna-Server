using NatournaServer.Constants.Error;
using NatournaServer.Constants.Log;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Models.Api.Requests.Bill;
using NatournaServer.Models.Api.Response.Bill;
using NatournaServer.Models.Entities;

namespace NatournaServer.Services.Api
{
    public class BillApiManager : IBillApiManager
    {
        private readonly IBillContextManager _billContextManager;
        private readonly IBalanceContextManager _balanceContextManager;
        private readonly IAuditService _auditService;
        private readonly ILogger<BillApiManager> _logger;

        public BillApiManager(IBillContextManager billContextManager, IBalanceContextManager balanceContextManager, IAuditService auditService, ILogger<BillApiManager> logger)
        {
            _billContextManager = billContextManager;
            _balanceContextManager = balanceContextManager;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<List<BillResponse>> GetAllBillsAsync()
        {
            List<BillEntity> bills = await _billContextManager.GetAllAsync();
            return bills.Select(MapToResponse).ToList();
        }

        public async Task<BillResponse?> GetBillByIdAsync(int id)
        {
            BillEntity? bill = await _billContextManager.GetByIdAsync(id);
            return bill == null ? null : MapToResponse(bill);
        }

        public async Task<BillResponse> CreateBillAsync(BillRequest bill)
        {
            BillEntity billEntity = new(bill.Label, bill.Amount, bill.BalanceId)
            {
                DueDate = bill.DueDate,
                IsPaid = false
            };

            BillEntity created = await _billContextManager.CreateAsync(billEntity);

            await _auditService.LogAsync(LogAction.Create, "Bill", created.Id, null, new { created.Amount, created.BalanceId, created.IsPaid });

            return MapToResponse(created);
        }

        public async Task<BillResponse?> UpdateBillAsync(int id, BillEntity bill)
        {
            BillEntity? existing = await _billContextManager.GetByIdAsync(id);

            if (existing == null)
                return null;

            var oldValues = new
            {
                existing.Amount,
                existing.BalanceId,
                existing.IsPaid
            };

            BillEntity? updated = await _billContextManager.UpdateAsync(id, bill);

            if (updated != null)
            {
                await _auditService.LogAsync(LogAction.Update, "Bill", id, oldValues, new { updated.Amount, updated.BalanceId, updated.IsPaid });

                return MapToResponse(updated);
            }

            return null;
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

        public async Task<BillResponse> MarkBillAsPaidAsync(int billId)
        {
            try
            {
                _logger.LogInformation("Marking bill {BillId} as paid", billId);

                BillEntity? bill = await _billContextManager.GetByIdAsync(billId);
                if (bill == null)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Bill.BillNotFound(billId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_NOT_FOUND_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_NOT_FOUND_ERROR, userMessage, technicalDetails);
                }

                if (bill.IsPaid)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Bill.AlreadyPaid(billId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_ALREADY_PAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_ALREADY_PAID_ERROR, userMessage, technicalDetails);
                }

                List<BalanceEntity> balances = await _balanceContextManager.GetAllAsync(balanceId: bill.BalanceId);
                BalanceEntity? balance = balances.FirstOrDefault();

                if (balance == null)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Balance.NotFound(bill.BalanceId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage, technicalDetails);
                }

                if (balance.CurrentAmount < bill.Amount)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Bill.InsufficientBalance(billId, balance.Id, bill.Amount, balance.CurrentAmount);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_INSUFFICIENT_BALANCE_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_INSUFFICIENT_BALANCE_ERROR, userMessage, technicalDetails);
                }

                balance.CurrentAmount -= bill.Amount;
                balance.UpdatedAt = DateTime.UtcNow;
                BalanceEntity? updatedBalance = await _balanceContextManager.UpdateAsync(balance.Id, balance);

                if (updatedBalance == null)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Balance.UpdateFailed(balance.Id, balance);
                    _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_UPDATE_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BALANCE_UPDATE_ERROR, userMessage, technicalDetails);
                }

                bill.IsPaid = true;
                bill.PaymentDate = DateTime.UtcNow;
                bill.UpdatedAt = DateTime.UtcNow;
                BillEntity? updatedBill = await _billContextManager.UpdateAsync(billId, bill);

                if (updatedBill == null)
                {
                    balance.CurrentAmount += bill.Amount;
                    await _balanceContextManager.UpdateAsync(balance.Id, balance);

                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Bill.MarkAsPaidFailed(billId);
                    _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_MARK_AS_PAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_MARK_AS_PAID_ERROR, userMessage, technicalDetails);
                }

                await _auditService.LogAsync(LogAction.Update, "Bill", billId, new { IsPaid = false, PaymentDate = (DateTime?)null }, new { IsPaid = true, bill.PaymentDate });

                _logger.LogInformation("Successfully marked bill {BillId} as paid on {PaymentDate} and deducted {Amount:C} from balance {BalanceId}",
                    billId, bill.PaymentDate, bill.Amount, balance.Id);

                return MapToResponse(updatedBill);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Bill.MarkAsPaidFailed(billId);
                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_MARK_AS_PAID_ERROR, userMessage);
                throw new ApiException(ErrorCodes.BILL_MARK_AS_PAID_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<BillResponse> MarkBillAsUnpaidAsync(int billId)
        {
            try
            {
                _logger.LogInformation("Marking bill {BillId} as unpaid", billId);

                BillEntity? bill = await _billContextManager.GetByIdAsync(billId);
                if (bill == null)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Bill.BillNotFound(billId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_NOT_FOUND_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_NOT_FOUND_ERROR, userMessage, technicalDetails);
                }

                if (!bill.IsPaid)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Bill.AlreadyUnpaid(billId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_ALREADY_UNPAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_ALREADY_UNPAID_ERROR, userMessage, technicalDetails);
                }

                List<BalanceEntity> balances = await _balanceContextManager.GetAllAsync(balanceId: bill.BalanceId);
                BalanceEntity? balance = balances.FirstOrDefault();

                if (balance == null)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Balance.NotFound(bill.BalanceId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage, technicalDetails);
                }

                balance.CurrentAmount += bill.Amount;
                balance.UpdatedAt = DateTime.UtcNow;

                BalanceEntity? updatedBalance = await _balanceContextManager.UpdateAsync(balance.Id, balance);

                if (updatedBalance == null)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Balance.UpdateFailed(balance.Id, balance);
                    _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_UPDATE_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BALANCE_UPDATE_ERROR, userMessage, technicalDetails);
                }

                bill.IsPaid = false;
                bill.PaymentDate = null;
                bill.UpdatedAt = DateTime.UtcNow;

                BillEntity? updatedBill = await _billContextManager.UpdateAsync(billId, bill);

                if (updatedBill == null)
                {
                    balance.CurrentAmount -= bill.Amount;
                    await _balanceContextManager.UpdateAsync(balance.Id, balance);

                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Bill.MarkAsUnpaidFailed(billId);
                    _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_MARK_AS_UNPAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_MARK_AS_UNPAID_ERROR, userMessage, technicalDetails);
                }

                await _auditService.LogAsync(LogAction.Update, "Bill", billId, new { IsPaid = true, bill.PaymentDate }, new { IsPaid = false, PaymentDate = (DateTime?)null });

                _logger.LogInformation("Successfully marked bill {BillId} as unpaid and added {Amount:C} back to balance {BalanceId}",
                    billId, bill.Amount, balance.Id);

                return MapToResponse(updatedBill);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Bill.MarkAsUnpaidFailed(billId);
                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_MARK_AS_UNPAID_ERROR, userMessage);
                throw new ApiException(ErrorCodes.BILL_MARK_AS_UNPAID_ERROR, userMessage, technicalDetails, ex);
            }
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
