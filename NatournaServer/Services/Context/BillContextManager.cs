using NatournaServer.Constants.Error;
using NatournaServer.Data;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Context;
using NatournaServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace NatournaServer.Services.Context
{
    public class BillContextManager : IBillContextManager
    {
        private readonly NatournaServerContext _context;
        private readonly ILogger<BillContextManager> _logger;

        public BillContextManager(NatournaServerContext context, ILogger<BillContextManager> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(List<BillEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, bool? isPaid = null)
        {
            try
            {
                var query = _context.Bills.Include(b => b.Balance).AsQueryable();

                if (isPaid.HasValue)
                {
                    query = query.Where(b => b.IsPaid == isPaid.Value);
                }

                int totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(b => b.DueDate)
                    .ThenByDescending(b => b.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Bill.GetAllFailed(null, isPaid, null, null);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}. {TechnicalDetails}", ErrorCodes.BILL_GET_ALL_ERROR, userMessage, technicalDetails);

                throw new ContextException(ErrorCodes.BILL_GET_ALL_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<BillEntity?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Bills
                    .Include(b => b.Balance)
                    .FirstOrDefaultAsync(b => b.Id == id);
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Bill.GetByIdFailed(id);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_GET_BY_ID_ERROR, userMessage);

                throw new ContextException(ErrorCodes.BILL_GET_BY_ID_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<BillEntity> CreateAsync(BillEntity bill)
        {
            try
            {
                _context.Bills.Add(bill);

                await _context.SaveChangesAsync();

                return await _context.Bills
                    .Include(b => b.Balance)
                    .FirstAsync(b => b.Id == bill.Id);
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Bill.CreateFailed(bill);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_CREATE_ERROR, userMessage);

                throw new ContextException(ErrorCodes.BILL_CREATE_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<BillEntity?> UpdateAsync(int id, string label, decimal amount, DateTime? dueDate)
        {
            try
            {
                var existingBill = await _context.Bills.FindAsync(id);
                if (existingBill == null)
                {
                    return null;
                }

                existingBill.Label = label;
                existingBill.Amount = amount;
                existingBill.DueDate = dueDate;
                existingBill.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await _context.Bills
                    .Include(b => b.Balance)
                    .FirstAsync(b => b.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to update bill with ID {BillId}", ErrorCodes.BILL_UPDATE_ERROR, id);

                throw new ContextException(ErrorCodes.BILL_UPDATE_ERROR, $"Failed to update bill with ID {id}", $"BillId: {id}", ex);
            }
        }

        public async Task<BillEntity?> SetPaidStatusAsync(int id, bool isPaid, DateTime? paymentDate)
        {
            try
            {
                var existingBill = await _context.Bills.FindAsync(id);
                if (existingBill == null)
                {
                    return null;
                }

                existingBill.IsPaid = isPaid;
                existingBill.PaymentDate = paymentDate;
                existingBill.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await _context.Bills
                    .Include(b => b.Balance)
                    .FirstAsync(b => b.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to update paid status for bill with ID {BillId}", ErrorCodes.BILL_UPDATE_ERROR, id);

                throw new ContextException(ErrorCodes.BILL_UPDATE_ERROR, $"Failed to update bill with ID {id}", $"BillId: {id}", ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var bill = await _context.Bills.FindAsync(id);
                if (bill == null)
                {
                    return false;
                }

                _context.Bills.Remove(bill);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Bill.DeleteFailed(id);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_DELETE_ERROR, userMessage);

                throw new ContextException(ErrorCodes.BILL_DELETE_ERROR, userMessage, technicalDetails, ex);
            }
        }
    }
}
