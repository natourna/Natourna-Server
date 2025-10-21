using BuildingManagement.Constants.Log;
using BuildingManagement.Interfaces.Api;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Interfaces.Services;
using BuildingManagement.Models.Api.Response.Building;
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

        public async Task<List<BuildingResponse>> GetAllBuildingsAsync()
        {
            List<BuildingEntity> buildings = await _contextManager.GetAllAsync();
            return buildings.Select(MapToResponse).ToList();
        }

        public async Task<BuildingResponse?> GetBuildingByIdAsync(int id)
        {
            BuildingEntity? building = await _contextManager.GetByIdAsync(id);
            return building == null ? null : MapToResponse(building);
        }

        public async Task<List<BuildingResponse>> GetBuildingsByCompoundIdAsync(int compoundId)
        {
            List<BuildingEntity> buildings = await _contextManager.GetByCompoundIdAsync(compoundId);
            return buildings.Select(MapToResponse).ToList();
        }

        public async Task<BuildingResponse> CreateBuildingAsync(BuildingEntity building)
        {
            var created = await _contextManager.CreateAsync(building);

            await _auditService.LogAsync(LogAction.Create, "Building", created.Id, null, new
            {
                created.Name,
                created.CompoundId
            });

            return MapToResponse(created);
        }

        public async Task<BuildingResponse?> UpdateBuildingAsync(int id, BuildingEntity building)
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

                return MapToResponse(updated);
            }

            return null;
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

        private static BuildingResponse MapToResponse(BuildingEntity building)
        {
            return new BuildingResponse
            {
                Id = building.Id,
                Name = building.Name,
                NumberOfApartments = building.NumberOfApartments,
                Floors = building.Floors,
                ActiveApartments = building.Apartments.Count(x => x.IsActive == true),
                CompoundId = building.CompoundId,
                CompoundName = building.Compound?.Name,
                CreatedAt = building.CreatedAt,
                UpdatedAt = building.UpdatedAt
            };
        }
    }
}