using BuildingManagement.Data;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuildingManagement.Services.Context
{
    public class ApartmentContextManager : IApartmentContextManager
    {
        private readonly BuildingManagementContext _context;

        public ApartmentContextManager(BuildingManagementContext context)
        {
            _context = context;
        }

        public async Task<List<ApartementEntity>> GetAllAsync()
        {
            return await _context.Apartements
                .Include(a => a.Building)
                .Include(a => a.User)
                .Include(a => a.Payments)
                .ToListAsync();
        }

        public async Task<ApartementEntity?> GetByIdAsync(int id)
        {
            return await _context.Apartements
                .Include(a => a.Building)
                .Include(a => a.User)
                .Include(a => a.Payments)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<ApartementEntity>> GetByBuildingIdAsync(int buildingId)
        {
            return await _context.Apartements
                .Include(a => a.User)
                .Include(a => a.Payments)
                .Where(a => a.BuildingId == buildingId)
                .ToListAsync();
        }

        public async Task<List<ApartementEntity>> GetByUserIdAsync(int userId)
        {
            return await _context.Apartements
                .Include(a => a.Building)
                .Include(a => a.Payments)
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }

        public async Task<ApartementEntity> CreateAsync(ApartementEntity apartment)
        {
            _context.Apartements.Add(apartment);
            await _context.SaveChangesAsync();
            return apartment;
        }

        public async Task<ApartementEntity?> UpdateAsync(int id, ApartementEntity apartment)
        {
            var existingApartment = await _context.Apartements.FindAsync(id);
            if (existingApartment == null)
                return null;

            existingApartment.AppartementNumber = apartment.AppartementNumber;
            existingApartment.Owner = apartment.Owner;
            existingApartment.Tenant = apartment.Tenant;
            existingApartment.Status = apartment.Status;
            existingApartment.BuildingId = apartment.BuildingId;
            existingApartment.UserId = apartment.UserId;
            existingApartment.UpdatededAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingApartment;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var apartment = await _context.Apartements.FindAsync(id);
            if (apartment == null)
                return false;

            _context.Apartements.Remove(apartment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}