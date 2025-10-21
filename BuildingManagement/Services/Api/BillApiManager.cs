using BuildingManagement.Constants.Error;
using BuildingManagement.Constants.Log;
using BuildingManagement.Exceptions;
using BuildingManagement.Interfaces.Api;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Interfaces.Services;
using BuildingManagement.Models.Api.Requests.Bill;
using BuildingManagement.Models.Entities;

namespace BuildingManagement.Services.Api
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

        public async Task<List<BillEntity>> GetAllBillsAsync()
        {
            return await _billContextManager.GetAllAsync();
        }

        public async Task<BillEntity?> GetBillByIdAsync(int id)
        {
            return await _billContextManager.GetByIdAsync(id);
        }

        public async Task<BillEntity> CreateBillAsync(BillRequest bill)
        {
            BillEntity billEntity = new(bill.Label, bill.Amount, bill.BalanceId)
            {
                DueDate = bill.DueDate,
                IsPaid = false
            };

            var created = await _billContextManager.CreateAsync(billEntity);

            await _auditService.LogAsync(LogAction.Create, "Bill", created.Id, null, new
            {
                created.Amount,
                created.BalanceId,
                created.IsPaid
            });

            return created;
        }

        public async Task<BillEntity?> UpdateBillAsync(int id, BillEntity bill)
        {
            var existing = await GetBillByIdAsync(id);
            if (existing == null)
                return null;

            var oldValues = new
            {
                existing.Amount,
                existing.BalanceId,
                existing.IsPaid
            };

            var updated = await _billContextManager.UpdateAsync(id, bill);

            if (updated != null)
            {
                await _auditService.LogAsync(LogAction.Update, "Bill", id, oldValues, new
                {
                    updated.Amount,
                    updated.BalanceId,
                    updated.IsPaid
                });
            }

            return updated;
        }

        public async Task<bool> DeleteBillAsync(int id)
        {
            var existing = await GetBillByIdAsync(id);
            if (existing == null)
                return false;

            await _auditService.LogAsync(LogAction.Delete, "Bill", id, new
            {
                existing.Amount,
                existing.BalanceId
            }, null);

            return await _billContextManager.DeleteAsync(id);
        }

        public async Task<BillEntity> MarkBillAsPaidAsync(int billId)
        {
            try
            {
                _logger.LogInformation("Marking bill {BillId} as paid", billId);

                var bill = await GetBillByIdAsync(billId);
                if (bill == null)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.BillNotFound(billId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_NOT_FOUND_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_NOT_FOUND_ERROR, userMessage, technicalDetails);
                }

                if (bill.IsPaid)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.AlreadyPaid(billId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_ALREADY_PAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_ALREADY_PAID_ERROR, userMessage, technicalDetails);
                }

                var balances = await _balanceContextManager.GetAllAsync(balanceId: bill.BalanceId);
                var balance = balances.FirstOrDefault();

                if (balance == null)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Balance.NotFound(bill.BalanceId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage, technicalDetails);
                }

                if (balance.CurrentAmount < bill.Amount)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.InsufficientBalance(billId, balance.Id, bill.Amount, balance.CurrentAmount);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_INSUFFICIENT_BALANCE_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_INSUFFICIENT_BALANCE_ERROR, userMessage, technicalDetails);
                }

                balance.CurrentAmount -= bill.Amount;
                balance.UpdatedAt = DateTime.UtcNow;
                var updatedBalance = await _balanceContextManager.UpdateAsync(balance.Id, balance);

                if (updatedBalance == null)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Balance.UpdateFailed(balance.Id, balance);
                    _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_UPDATE_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BALANCE_UPDATE_ERROR, userMessage, technicalDetails);
                }

                bill.IsPaid = true;
                bill.PaymentDate = DateTime.UtcNow;
                bill.UpdatedAt = DateTime.UtcNow;
                var updatedBill = await _billContextManager.UpdateAsync(billId, bill);

                if (updatedBill == null)
                {
                    balance.CurrentAmount += bill.Amount;
                    await _balanceContextManager.UpdateAsync(balance.Id, balance);

                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.MarkAsPaidFailed(billId);
                    _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_MARK_AS_PAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_MARK_AS_PAID_ERROR, userMessage, technicalDetails);
                }

                await _auditService.LogAsync(LogAction.Update, "Bill", billId,
                    new { IsPaid = false, PaymentDate = (DateTime?)null },
                    new { IsPaid = true, bill.PaymentDate });

                _logger.LogInformation("Successfully marked bill {BillId} as paid on {PaymentDate} and deducted {Amount:C} from balance {BalanceId}",
                    billId, bill.PaymentDate, bill.Amount, balance.Id);

                return updatedBill;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.MarkAsPaidFailed(billId);
                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_MARK_AS_PAID_ERROR, userMessage);
                throw new ApiException(ErrorCodes.BILL_MARK_AS_PAID_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<BillEntity> MarkBillAsUnpaidAsync(int billId)
        {
            try
            {
                _logger.LogInformation("Marking bill {BillId} as unpaid", billId);

                var bill = await GetBillByIdAsync(billId);
                if (bill == null)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.BillNotFound(billId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_NOT_FOUND_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_NOT_FOUND_ERROR, userMessage, technicalDetails);
                }

                if (!bill.IsPaid)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.AlreadyUnpaid(billId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_ALREADY_UNPAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_ALREADY_UNPAID_ERROR, userMessage, technicalDetails);
                }

                var balances = await _balanceContextManager.GetAllAsync(balanceId: bill.BalanceId);
                var balance = balances.FirstOrDefault();

                if (balance == null)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Balance.NotFound(bill.BalanceId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage, technicalDetails);
                }

                balance.CurrentAmount += bill.Amount;
                balance.UpdatedAt = DateTime.UtcNow;
                var updatedBalance = await _balanceContextManager.UpdateAsync(balance.Id, balance);

                if (updatedBalance == null)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Balance.UpdateFailed(balance.Id, balance);
                    _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_UPDATE_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BALANCE_UPDATE_ERROR, userMessage, technicalDetails);
                }

                bill.IsPaid = false;
                bill.PaymentDate = null;
                bill.UpdatedAt = DateTime.UtcNow;
                var updatedBill = await _billContextManager.UpdateAsync(billId, bill);

                if (updatedBill == null)
                {
                    balance.CurrentAmount -= bill.Amount;
                    await _balanceContextManager.UpdateAsync(balance.Id, balance);

                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.MarkAsUnpaidFailed(billId);
                    _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_MARK_AS_UNPAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_MARK_AS_UNPAID_ERROR, userMessage, technicalDetails);
                }

                await _auditService.LogAsync(LogAction.Update, "Bill", billId,
                    new { IsPaid = true, PaymentDate = bill.PaymentDate },
                    new { IsPaid = false, PaymentDate = (DateTime?)null });

                _logger.LogInformation("Successfully marked bill {BillId} as unpaid and added {Amount:C} back to balance {BalanceId}",
                    billId, bill.Amount, balance.Id);

                return updatedBill;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.MarkAsUnpaidFailed(billId);
                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_MARK_AS_UNPAID_ERROR, userMessage);
                throw new ApiException(ErrorCodes.BILL_MARK_AS_UNPAID_ERROR, userMessage, technicalDetails, ex);
            }
        }
    }
}
