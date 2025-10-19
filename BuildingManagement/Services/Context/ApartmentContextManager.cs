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

        public async Task<List<ApartmentEntity>> GetAllAsync(int? apartmentId = null, int? buildingId = null, bool? isActive = null)
        {
            var query = _context.Apartments
                .Include(a => a.Building)
                .AsQueryable();

            // Apply filters
            if (apartmentId.HasValue)
            {
                query = query.Where(a => a.Id == apartmentId.Value);
            }

            if (buildingId.HasValue)
            {
                query = query.Where(a => a.BuildingId == buildingId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(a => a.IsActive == isActive.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<ApartmentEntity?> GetByIdAsync(int id)
        {
            return await _context.Apartments
                .Include(a => a.Building)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<ApartmentEntity>> GetByBuildingIdAsync(int buildingId)
        {
            return await _context.Apartments
                .Include(a => a.Building)
                .Where(a => a.BuildingId == buildingId)
                .ToListAsync();
        }

        public async Task<ApartmentEntity> CreateAsync(ApartmentEntity apartment)
        {
            _context.Apartments.Add(apartment);
            await _context.SaveChangesAsync();
            
            // Reload with building information
            return await _context.Apartments
                .Include(a => a.Building)
                .FirstAsync(a => a.Id == apartment.Id);
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
            existingApartment.UpdatededAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            // Reload with building information
            return await _context.Apartments
                .Include(a => a.Building)
                .FirstAsync(a => a.Id == id);
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

        public async Task<ApartmentEntity?> SetActiveAsync(int id, bool isActive)
        {
            var apartment = await _context.Apartments.FindAsync(id);
            if (apartment == null)
                return null;

            apartment.IsActive = isActive;
            apartment.UpdatededAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            // Reload with building information
            return await _context.Apartments
                .Include(a => a.Building)
                .FirstAsync(a => a.Id == id);
        }
    }
}