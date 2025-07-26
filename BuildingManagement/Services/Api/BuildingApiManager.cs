using BuildingManagement.Interfaces.Api;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Models.Entities;

namespace BuildingManagement.Services.Api
{
    public class BuildingApiManager : IBuildingApiManager
    {
        private readonly IBuildingContextManager _contextManager;

        public BuildingApiManager(IBuildingContextManager contextManager)
        {
            _contextManager = contextManager;
        }

        public async Task<List<BuildingEntity>> GetAllBuildingsAsync()
        {
            return await _contextManager.GetAllAsync();
        }

        public async Task<BuildingEntity?> GetBuildingByIdAsync(int id)
        {
            return await _contextManager.GetByIdAsync(id);
        }

        public async Task<List<BuildingEntity>> GetBuildingsByCompoundIdAsync(int compoundId)
        {
            return await _contextManager.GetByCompoundIdAsync(compoundId);
        }

        public async Task<BuildingEntity> CreateBuildingAsync(BuildingEntity building)
        {
            return await _contextManager.CreateAsync(building);
        }

        public async Task<BuildingEntity?> UpdateBuildingAsync(int id, BuildingEntity building)
        {
            return await _contextManager.UpdateAsync(id, building);
        }

        public async Task<bool> DeleteBuildingAsync(int id)
        {
            return await _contextManager.DeleteAsync(id);
        }
    }
}