using NatrounaServer.Constants.Error;
using NatrounaServer.Data;
using NatrounaServer.Exceptions;
using NatrounaServer.Interfaces.Context;
using NatrounaServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace NatrounaServer.Services.Context
{
    public class PaymentContextManager : IPaymentContextManager
    {
        private readonly NatrounaServerContext _context;
        private readonly ILogger<PaymentContextManager> _logger;

        public PaymentContextManager(NatrounaServerContext context, ILogger<PaymentContextManager> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PaymentEntity>> GetAllAsync(int? paymentId = null, int? apartmentId = null, int? cycleId = null, bool? isPaid = null)
        {
            try
            {
                _logger.LogInformation("Getting all payments with filters - PaymentId: {PaymentId}, ApartmentId: {ApartmentId}, CycleId: {CycleId}, IsPaid: {IsPaid}", paymentId, apartmentId, cycleId, isPaid);

                var query = _context.Payments.Include(p => p.Apartment).Include(p => p.Cycle).Include(p => p.PaymentAllocations).AsQueryable();

                // Apply filters
                if (paymentId.HasValue)
                {
                    query = query.Where(p => p.Id == paymentId.Value);
                }

                if (apartmentId.HasValue)
                {
                    query = query.Where(p => p.ApartmentId == apartmentId.Value);
                }

                if (cycleId.HasValue)
                {
                    query = query.Where(p => p.CycleId == cycleId.Value);
                }

                if (isPaid.HasValue)
                {
                    query = query.Where(p => p.IsPaid == isPaid.Value);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.GetAllFailed(paymentId, apartmentId, cycleId, isPaid);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}. {TechnicalDetails}", ErrorCodes.PAYMENT_GET_ALL_ERROR, userMessage, technicalDetails);

                throw new ContextException(ErrorCodes.PAYMENT_GET_ALL_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<PaymentEntity?> GetByIdAsync(int id)
        {
            try
            {
                return (await GetAllAsync(paymentId: id)).FirstOrDefault();
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
                _logger.LogInformation("Creating new payment - Amount: {Amount}, ApartmentId: {ApartmentId}, CycleId: {CycleId}", payment.Amount, payment.ApartmentId, payment.CycleId);

                _context.Payments.Add(payment);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created payment with ID {PaymentId}", payment.Id);

                return payment;
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.CreateFailed(payment);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_CREATE_ERROR, userMessage);

                throw new ContextException(ErrorCodes.PAYMENT_CREATE_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<PaymentEntity?> UpdateAsync(int id, PaymentEntity payment)
        {
            try
            {
                _logger.LogInformation("Updating payment with ID: {PaymentId}", id);

                var existingPayment = await _context.Payments.FindAsync(id);
                if (existingPayment == null)
                {
                    _logger.LogWarning("Cannot update - Payment with ID {PaymentId} not found", id);
                    return null;
                }

                existingPayment.PaymentDate = payment.PaymentDate;
                existingPayment.Amount = payment.Amount;
                existingPayment.ApartmentId = payment.ApartmentId;
                existingPayment.DueDate = payment.DueDate;
                existingPayment.IsPaid = payment.IsPaid;
                existingPayment.CycleId = payment.CycleId;
                existingPayment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated payment with ID {PaymentId}", id);

                return existingPayment;
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Payment.UpdateFailed(id, payment);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.PAYMENT_UPDATE_ERROR, userMessage);

                throw new ContextException(ErrorCodes.PAYMENT_UPDATE_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting payment with ID: {PaymentId}", id);

                var payment = await _context.Payments.FindAsync(id);
                if (payment == null)
                {
                    _logger.LogWarning("Cannot delete - Payment with ID {PaymentId} not found", id);
                    return false;
                }

                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted payment with ID {PaymentId}", id);

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