using BuildingManagement.Data;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuildingManagement.Services.Context
{
    public class BillContextManager : IBillContextManager
    {
        private readonly BuildingManagementContext _context;

        public BillContextManager(BuildingManagementContext context)
        {
            _context = context;
        }

        public async Task<List<BillEntity>> GetAllAsync()
        {
            return await _context.Bills
                .Include(b => b.Compound)
                .Include(b => b.Payments)
                .ToListAsync();
        }

        public async Task<BillEntity?> GetByIdAsync(int id)
        {
            return await _context.Bills
                .Include(b => b.Compound)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<BillEntity>> GetByCompoundIdAsync(int compoundId)
        {
            return await _context.Bills
                .Include(b => b.Payments)
                .Where(b => b.CompoundId == compoundId)
                .ToListAsync();
        }

        public async Task<BillEntity> CreateAsync(BillEntity bill)
        {
            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();
            return bill;
        }

        public async Task<BillEntity?> UpdateAsync(int id, BillEntity bill)
        {
            var existingBill = await _context.Bills.FindAsync(id);
            if (existingBill == null)
                return null;

            existingBill.Amount = bill.Amount;
            existingBill.DueDate = bill.DueDate;
            existingBill.CompoundId = bill.CompoundId;
            existingBill.UpdatededAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingBill;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill == null)
                return false;

            _context.Bills.Remove(bill);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}