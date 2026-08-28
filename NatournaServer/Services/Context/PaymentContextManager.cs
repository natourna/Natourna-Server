using NatournaServer.Constants.Error;
using NatournaServer.Data;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Context;
using NatournaServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace NatournaServer.Services.Context
{
    public class PaymentContextManager : IPaymentContextManager
    {
        private readonly NatournaServerContext _context;
        private readonly ILogger<PaymentContextManager> _logger;

        public PaymentContextManager(NatournaServerContext context, ILogger<PaymentContextManager> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(List<PaymentEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, int? apartmentId = null, bool? isPaid = null, DateTime? dueBefore = null)
        {
            try
            {
                var query = _context.Payments
                    .Include(p => p.Apartment)
                    .Include(p => p.Cycle)
                    .Include(p => p.PaymentAllocations)
                    .ThenInclude(pa => pa.Balance)
                    .AsQueryable();

                if (apartmentId.HasValue)
                {
                    query = query.Where(p => p.ApartmentId == apartmentId.Value);
                }

                if (isPaid.HasValue)
                {
                    query = query.Where(p => p.IsPaid == isPaid.Value);
                }

                if (dueBefore.HasValue)
                {
                    query = query.Where(p => p.DueDate != null && p.DueDate < dueBefore.Value);
                }

                int totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(p => p.DueDate)
                    .ThenByDescending(p => p.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.GetAllFailed(null, apartmentId, null, isPaid);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}. {TechnicalDetails}", ErrorCodes.PAYMENT_GET_ALL_ERROR, userMessage, technicalDetails);

                throw new ContextException(ErrorCodes.PAYMENT_GET_ALL_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<PaymentEntity?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Payments
                    .Include(p => p.Apartment)
                    .Include(p => p.Cycle)
                    .Include(p => p.PaymentAllocations)
                    .ThenInclude(pa => pa.Balance)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.GetByIdFailed(id);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_GET_BY_ID_ERROR, userMessage);

                throw new ContextException(ErrorCodes.PAYMENT_GET_BY_ID_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<PaymentEntity> CreateAsync(PaymentEntity payment)
        {
            try
            {
                _context.Payments.Add(payment);

                await _context.SaveChangesAsync();

                return payment;
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.CreateFailed(payment);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_CREATE_ERROR, userMessage);

                throw new ContextException(ErrorCodes.PAYMENT_CREATE_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<PaymentEntity?> UpdateAsync(int id, string label, decimal amount, DateTime? dueDate, int apartmentId)
        {
            try
            {
                var existingPayment = await _context.Payments.FindAsync(id);
                if (existingPayment == null)
                {
                    return null;
                }

                existingPayment.Label = label;
                existingPayment.Amount = amount;
                existingPayment.DueDate = dueDate;
                existingPayment.ApartmentId = apartmentId;
                existingPayment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to update payment with ID {PaymentId}", ErrorCodes.PAYMENT_UPDATE_ERROR, id);

                throw new ContextException(ErrorCodes.PAYMENT_UPDATE_ERROR, $"Failed to update payment with ID {id}", $"PaymentId: {id}", ex);
            }
        }

        public async Task<PaymentEntity?> SetPaidStatusAsync(int id, bool isPaid, DateTime? paymentDate)
        {
            try
            {
                var existingPayment = await _context.Payments.FindAsync(id);
                if (existingPayment == null)
                {
                    return null;
                }

                existingPayment.IsPaid = isPaid;
                existingPayment.PaymentDate = paymentDate;
                existingPayment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to update paid status for payment with ID {PaymentId}", ErrorCodes.PAYMENT_UPDATE_ERROR, id);

                throw new ContextException(ErrorCodes.PAYMENT_UPDATE_ERROR, $"Failed to update payment with ID {id}", $"PaymentId: {id}", ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var payment = await _context.Payments.FindAsync(id);
                if (payment == null)
                {
                    return false;
                }

                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.DeleteFailed(id);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_DELETE_ERROR, userMessage);

                throw new ContextException(ErrorCodes.PAYMENT_DELETE_ERROR, userMessage, technicalDetails, ex);
            }
        }
    }
}
