using BuildingManagement.Constants;
using BuildingManagement.Exceptions;
using BuildingManagement.Interfaces.Api;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Models.Entities;

namespace BuildingManagement.Services.Api
{
    public class BillApiManager : IBillApiManager
    {
        private readonly IBillContextManager _billContextManager;
        private readonly IBalanceContextManager _balanceContextManager;
        private readonly ILogger<BillApiManager> _logger;

        public BillApiManager(IBillContextManager billContextManager, IBalanceContextManager balanceContextManager, ILogger<BillApiManager> logger)
        {
            _billContextManager = billContextManager;
            _balanceContextManager = balanceContextManager;
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

        public async Task<BillEntity> CreateBillAsync(BillEntity bill)
        {
            return await _billContextManager.CreateAsync(bill);
        }

        public async Task<BillEntity?> UpdateBillAsync(int id, BillEntity bill)
        {
            return await _billContextManager.UpdateAsync(id, bill);
        }

        public async Task<bool> DeleteBillAsync(int id)
        {
            return await _billContextManager.DeleteAsync(id);
        }

        public async Task<BillEntity> MarkBillAsPaidAsync(int billId)
        {
            try
            {
                _logger.LogInformation("Marking bill {BillId} as paid", billId);

                // Get the bill
                var bill = await GetBillByIdAsync(billId);
                if (bill == null)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.BillNotFound(billId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_NOT_FOUND_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_NOT_FOUND_ERROR, userMessage, technicalDetails);
                }

                // Check if already paid
                if (bill.IsPaid == true)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.AlreadyPaid(billId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_ALREADY_PAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_ALREADY_PAID_ERROR, userMessage, technicalDetails);
                }

                // Get the balance
                var balances = await _balanceContextManager.GetAllAsync(balanceId: bill.BalanceId);
                var balance = balances.FirstOrDefault();

                if (balance == null)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Balance.NotFound(bill.BalanceId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage, technicalDetails);
                }

                // Check if sufficient funds
                if (balance.CurrentAmount < bill.Amount)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.InsufficientBalance(
                        billId, balance.Id, bill.Amount, balance.CurrentAmount);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_INSUFFICIENT_BALANCE_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_INSUFFICIENT_BALANCE_ERROR, userMessage, technicalDetails);
                }

                // Deduct amount from balance
                balance.CurrentAmount -= bill.Amount;
                balance.UpdatededAt = DateTime.UtcNow;
                var updatedBalance = await _balanceContextManager.UpdateAsync(balance.Id, balance);

                if (updatedBalance == null)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Balance.UpdateFailed(balance.Id, balance);
                    _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_UPDATE_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BALANCE_UPDATE_ERROR, userMessage, technicalDetails);
                }

                // Mark bill as paid with payment date
                bill.IsPaid = true;
                bill.PaymentDate = DateTime.UtcNow;
                bill.UpdatededAt = DateTime.UtcNow;
                var updatedBill = await UpdateBillAsync(billId, bill);

                if (updatedBill == null)
                {
                    // Rollback: Add amount back to balance
                    balance.CurrentAmount += bill.Amount;
                    await _balanceContextManager.UpdateAsync(balance.Id, balance);

                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.MarkAsPaidFailed(billId);
                    _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_MARK_AS_PAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_MARK_AS_PAID_ERROR, userMessage, technicalDetails);
                }

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

        /// <summary>
        /// Mark a bill as unpaid and add the amount back to the balance.
        /// Clears the PaymentDate.
        /// This allows users to correct mistakes when they accidentally mark a bill as paid.
        /// </summary>
        public async Task<BillEntity> MarkBillAsUnpaidAsync(int billId)
        {
            try
            {
                _logger.LogInformation("Marking bill {BillId} as unpaid", billId);

                // Get the bill
                var bill = await GetBillByIdAsync(billId);
                if (bill == null)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.BillNotFound(billId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_NOT_FOUND_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_NOT_FOUND_ERROR, userMessage, technicalDetails);
                }

                // Check if already unpaid
                if (bill.IsPaid == false)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.AlreadyUnpaid(billId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_ALREADY_UNPAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_ALREADY_UNPAID_ERROR, userMessage, technicalDetails);
                }

                // Get the balance
                var balances = await _balanceContextManager.GetAllAsync(balanceId: bill.BalanceId);
                var balance = balances.FirstOrDefault();

                if (balance == null)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Balance.NotFound(bill.BalanceId);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BALANCE_NOT_FOUND_ERROR, userMessage, technicalDetails);
                }

                // Add amount back to balance
                balance.CurrentAmount += bill.Amount;
                balance.UpdatededAt = DateTime.UtcNow;
                var updatedBalance = await _balanceContextManager.UpdateAsync(balance.Id, balance);

                if (updatedBalance == null)
                {
                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Balance.UpdateFailed(balance.Id, balance);
                    _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_UPDATE_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BALANCE_UPDATE_ERROR, userMessage, technicalDetails);
                }

                // Mark bill as unpaid and clear payment date
                bill.IsPaid = false;
                bill.PaymentDate = null;
                bill.UpdatededAt = DateTime.UtcNow;
                var updatedBill = await UpdateBillAsync(billId, bill);

                if (updatedBill == null)
                {
                    // Rollback: Deduct amount from balance
                    balance.CurrentAmount -= bill.Amount;
                    await _balanceContextManager.UpdateAsync(balance.Id, balance);

                    var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.MarkAsUnpaidFailed(billId);
                    _logger.LogError("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_MARK_AS_UNPAID_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.BILL_MARK_AS_UNPAID_ERROR, userMessage, technicalDetails);
                }

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
