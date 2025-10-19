using BuildingManagement.Constants.Log;
using BuildingManagement.Interfaces.Api;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Interfaces.Services;
using BuildingManagement.Models.Entities;

namespace BuildingManagement.Services.Api
{
    public class BuildingApiManager : IBuildingApiManager
    {
        private readonly IBuildingContextManager _contextManager;
        private readonly IAuditService _auditService;

        public BuildingApiManager(IBuildingContextManager contextManager, IAuditService auditService)
        {
            _contextManager = contextManager;
            _auditService = auditService;
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
            var created = await _contextManager.CreateAsync(building);

            await _auditService.LogAsync(LogAction.Create, "Building", created.Id, null, new
            {
                created.Name,
                created.CompoundId
            });

            return created;
        }

        public async Task<BuildingEntity?> UpdateBuildingAsync(int id, BuildingEntity building)
        {
            var existing = await GetBuildingByIdAsync(id);
            if (existing == null)
            {
                return null;
            }

            var oldValues = new
            {
                existing.Name,
                existing.CompoundId
            };

            var updated = await _contextManager.UpdateAsync(id, building);

            if (updated != null)
            {
                await _auditService.LogAsync(LogAction.Update, "Building", id, oldValues, new
                {
                    updated.Name,
                    updated.CompoundId
                });
            }

            return updated;
        }

        public async Task<bool> DeleteBuildingAsync(int id)
        {
            var existing = await GetBuildingByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            await _auditService.LogAsync(LogAction.Delete, "Building", id, new
            {
                existing.Name,
                existing.CompoundId
            }, null);

            return await _contextManager.DeleteAsync(id);
        }
    }
}