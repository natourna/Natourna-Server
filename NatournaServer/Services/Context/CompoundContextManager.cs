using NatournaServer.Data;
using NatournaServer.Interfaces.Context;
using NatournaServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace NatournaServer.Services.Context
{
    public class CompoundContextManager : ICompoundContextManager
    {
        private readonly NatournaServerContext _context;

        public CompoundContextManager(NatournaServerContext context)
        {
            _context = context;
        }

        public async Task<List<CompoundEntity>> GetAllAsync()
        {
            return await _context.Compounds
                .Include(c => c.Buildings)
                .ToListAsync();
        }

        public async Task<CompoundEntity?> GetByIdAsync(int id)
        {
            return await _context.Compounds
                .Include(c => c.Buildings)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CompoundEntity> CreateAsync(CompoundEntity compound)
        {
            _context.Compounds.Add(compound);
            await _context.SaveChangesAsync();
            return compound;
        }

        public async Task<CompoundEntity?> UpdateAsync(int id, CompoundEntity compound)
        {
            var existingCompound = await _context.Compounds.FindAsync(id);
            if (existingCompound == null)
                return null;

            existingCompound.Name = compound.Name;
            existingCompound.Address = compound.Address;
            existingCompound.ActiveApartments = compound.ActiveApartments;
            existingCompound.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingCompound;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var compound = await _context.Compounds.FindAsync(id);
            if (compound == null)
                return false;

            _context.Compounds.Remove(compound);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}