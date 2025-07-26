using BuildingManagement.Data;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuildingManagement.Services.Context
{
    public class PaymentContextManager : IPaymentContextManager
    {
        private readonly BuildingManagementContext _context;

        public PaymentContextManager(BuildingManagementContext context)
        {
            _context = context;
        }

        public async Task<List<PaymentEntity>> GetAllAsync()
        {
            return await _context.Payments
                .Include(p => p.Bill)
                .Include(p => p.Apartement)
                .ToListAsync();
        }

        public async Task<PaymentEntity?> GetByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Bill)
                .Include(p => p.Apartement)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<PaymentEntity>> GetByBillIdAsync(int billId)
        {
            return await _context.Payments
                .Include(p => p.Apartement)
                .Where(p => p.BillId == billId)
                .ToListAsync();
        }

        public async Task<List<PaymentEntity>> GetByApartmentIdAsync(int apartmentId)
        {
            return await _context.Payments
                .Include(p => p.Bill)
                .Where(p => p.ApartmentId == apartmentId)
                .ToListAsync();
        }

        public async Task<PaymentEntity> CreateAsync(PaymentEntity payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<PaymentEntity?> UpdateAsync(int id, PaymentEntity payment)
        {
            var existingPayment = await _context.Payments.FindAsync(id);
            if (existingPayment == null)
                return null;

            existingPayment.Recurrent = payment.Recurrent;
            existingPayment.PaymentDate = payment.PaymentDate;
            existingPayment.Amount = payment.Amount;
            existingPayment.BillId = payment.BillId;
            existingPayment.ApartmentId = payment.ApartmentId;
            existingPayment.UpdatededAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingPayment;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return false;

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}