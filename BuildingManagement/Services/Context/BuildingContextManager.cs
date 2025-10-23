using BuildingManagement.Data;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuildingManagement.Services.Context
{
    public class BuildingContextManager : IBuildingContextManager
    {
        private readonly BuildingManagementContext _context;

        public BuildingContextManager(BuildingManagementContext context)
        {
            _context = context;
        }

        public async Task<List<BuildingEntity>> GetAllAsync()
        {
            return await _context.Buildings
                .Include(b => b.Compound)
                .Include(b => b.Apartments)
                .ToListAsync();
        }

        public async Task<BuildingEntity?> GetByIdAsync(int id)
        {
            return await _context.Buildings
                .Include(b => b.Compound)
                .Include(b => b.Apartments)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<BuildingEntity>> GetByCompoundIdAsync(int compoundId)
        {
            return await _context.Buildings
                .Include(b => b.Apartments)
                .Where(b => b.CompoundId == compoundId)
                .ToListAsync();
        }

        public async Task<BuildingEntity> CreateAsync(BuildingEntity building)
        {
            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();
            return building;
        }

        public async Task<BuildingEntity?> UpdateAsync(int id, BuildingEntity building)
        {
            var existingBuilding = await _context.Buildings.FindAsync(id);
            if (existingBuilding == null)
                return null;

            existingBuilding.Name = building.Name;
            existingBuilding.NumberOfApartments = building.NumberOfApartments;
            existingBuilding.Floors = building.Floors;
            existingBuilding.CompoundId = building.CompoundId;
            existingBuilding.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingBuilding;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building == null)
                return false;

            _context.Buildings.Remove(building);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}