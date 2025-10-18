using BuildingManagement.Constants;
using BuildingManagement.Data;
using BuildingManagement.Exceptions;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuildingManagement.Services.Context
{
    public class BillContextManager : IBillContextManager
    {
        private readonly BuildingManagementContext _context;
        private readonly ILogger<BillContextManager> _logger;

        public BillContextManager(BuildingManagementContext context, ILogger<BillContextManager> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<BillEntity>> GetAllAsync(int? billId = null, int? balanceId = null, bool? isPaid = null, DateTime? dueDateFrom = null, DateTime? dueDateTo = null)
        {
            try
            {
                _logger.LogInformation("Getting all bills with filters - BalanceId: {BalanceId}, IsPaid: {IsPaid}, DueDateFrom: {DueDateFrom}, DueDateTo: {DueDateTo}", balanceId, isPaid, dueDateFrom, dueDateTo);

                var query = _context.Bills.AsQueryable();

                // Apply filters
                if (billId.HasValue)
                {
                    query = query.Where(b => b.Id == billId.Value);
                }

                if (balanceId.HasValue)
                {
                    query = query.Where(b => b.BalanceId == balanceId.Value);
                }

                if (isPaid.HasValue)
                {
                    query = query.Where(b => b.IsPaid == isPaid.Value);
                }

                if (dueDateFrom.HasValue)
                {
                    query = query.Where(b => b.DueDate >= dueDateFrom.Value);
                }

                if (dueDateTo.HasValue)
                {
                    query = query.Where(b => b.DueDate <= dueDateTo.Value);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.GetAllFailed(balanceId, isPaid, dueDateFrom, dueDateTo);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}. {TechnicalDetails}", ErrorCodes.BILL_GET_ALL_ERROR, userMessage, technicalDetails);

                throw new ContextException(ErrorCodes.BILL_GET_ALL_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<BillEntity?> GetByIdAsync(int id)
        {
            try
            {
                return (await GetAllAsync(billId: id)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.GetByIdFailed(id);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_GET_BY_ID_ERROR, userMessage);

                throw new ContextException(ErrorCodes.BILL_GET_BY_ID_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<List<BillEntity>> GetByBalanceIdAsync(int balanceId)
        {
            try
            {
                return (await GetAllAsync(balanceId: balanceId)).ToList();
            }
            catch (Exception ex)
            {
                var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.GetByBalanceIdFailed(balanceId);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_GET_ALL_ERROR, userMessage);

                throw new ContextException(ErrorCodes.BILL_GET_ALL_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<BillEntity> CreateAsync(BillEntity bill)
        {
            try
            {
                _logger.LogInformation("Creating new bill - Label: {Label}, Amount: {Amount}, BalanceId: {BalanceId}", bill.Label, bill.Amount, bill.BalanceId);

                _context.Bills.Add(bill);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created bill with ID {BillId}", bill.Id);

                return bill;
            }
            catch (Exception ex)
            {
                var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.CreateFailed(bill);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_CREATE_ERROR, userMessage);

                throw new ContextException(ErrorCodes.BILL_CREATE_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<BillEntity?> UpdateAsync(int id, BillEntity bill)
        {
            try
            {
                _logger.LogInformation("Updating bill with ID: {BillId}", id);

                var existingBill = await _context.Bills.FindAsync(id);
                if (existingBill == null)
                {
                    _logger.LogWarning("Cannot update - Bill with ID {BillId} not found", id);
                    return null;
                }

                existingBill.Label = bill.Label;
                existingBill.IsPaid = bill.IsPaid;
                existingBill.PaymentDate = bill.PaymentDate;
                existingBill.Amount = bill.Amount;
                existingBill.DueDate = bill.DueDate;
                existingBill.UpdatededAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated bill with ID {BillId}", id);

                return existingBill;
            }
            catch (Exception ex)
            {
                var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.UpdateFailed(id, bill);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_UPDATE_ERROR, userMessage);

                throw new ContextException(ErrorCodes.BILL_UPDATE_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting bill with ID: {BillId}", id);

                var bill = await _context.Bills.FindAsync(id);
                if (bill == null)
                {
                    _logger.LogWarning("Cannot delete - Bill with ID {BillId} not found", id);
                    return false;
                }

                _context.Bills.Remove(bill);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted bill with ID {BillId}", id);

                return true;
            }
            catch (Exception ex)
            {
                var (userMessage, technicalDetails) = ErrorMessageBuilder.Bill.DeleteFailed(id);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_DELETE_ERROR, userMessage);

                throw new ContextException(ErrorCodes.BILL_DELETE_ERROR, userMessage, technicalDetails, ex);
            }
        }
    }
}