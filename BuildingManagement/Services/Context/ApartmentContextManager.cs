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

        public async Task<List<ApartmentEntity>> GetAllAsync()
        {
            return await _context.Apartments
                .Include(a => a.Building)
                .Include(a => a.User)
                .Include(a => a.Payments)
                .ToListAsync();
        }

        public async Task<ApartmentEntity?> GetByIdAsync(int id)
        {
            return await _context.Apartments
                .Include(a => a.Building)
                .Include(a => a.User)
                .Include(a => a.Payments)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<ApartmentEntity>> GetByBuildingIdAsync(int buildingId)
        {
            return await _context.Apartments
                .Include(a => a.User)
                .Include(a => a.Payments)
                .Where(a => a.BuildingId == buildingId)
                .ToListAsync();
        }

        public async Task<List<ApartmentEntity>> GetByUserIdAsync(int userId)
        {
            return await _context.Apartments
                .Include(a => a.Building)
                .Include(a => a.Payments)
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }

        public async Task<ApartmentEntity> CreateAsync(ApartmentEntity apartment)
        {
            _context.Apartments.Add(apartment);
            await _context.SaveChangesAsync();
            return apartment;
        }

        public async Task<ApartmentEntity?> UpdateAsync(int id, ApartmentEntity apartment)
        {
            var existingApartment = await _context.Apartments.FindAsync(id);
            if (existingApartment == null)
                return null;

            existingApartment.ApartmentInfo = apartment.ApartmentInfo;
            existingApartment.Owner = apartment.Owner;
            existingApartment.Tenant = apartment.Tenant;
            existingApartment.IsActive = apartment.IsActive;
            existingApartment.BuildingId = apartment.BuildingId;
            existingApartment.Floor = apartment.Floor;
            existingApartment.UserId = apartment.UserId;
            existingApartment.UpdatededAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingApartment;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var apartment = await _context.Apartments.FindAsync(id);
            if (apartment == null)
                return false;

            _context.Apartments.Remove(apartment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}