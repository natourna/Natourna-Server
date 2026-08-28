using NatournaServer.Constants.Error;
using NatournaServer.Constants.Log;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Models.Api.Requests.Building;
using NatournaServer.Models.Api.Response.Building;
using NatournaServer.Models.Entities;

namespace NatournaServer.Services.Api
{
    public class BuildingApiManager : IBuildingApiManager
    {
        private readonly IBuildingContextManager _contextManager;
        private readonly ICompoundContextManager _compoundContextManager;
        private readonly IAuditService _auditService;

        public BuildingApiManager(IBuildingContextManager contextManager, ICompoundContextManager compoundContextManager, IAuditService auditService)
        {
            _contextManager = contextManager;
            _compoundContextManager = compoundContextManager;
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

        public async Task<BuildingResponse> CreateBuildingAsync(BuildingRequest request)
        {
            await EnsureCompoundExistsAsync(request.CompoundId);

            var building = new BuildingEntity(0, request.Name, request.NumberOfApartments, request.Floors, request.CompoundId);

            BuildingEntity created = await _contextManager.CreateAsync(building);

            await _auditService.LogAsync(LogAction.Create, "Building", created.Id, null, new { created.Name, created.CompoundId });

            return MapToResponse(created);
        }

        public async Task<BuildingResponse?> UpdateBuildingAsync(int id, BuildingRequest request)
        {
            BuildingEntity? existing = await _contextManager.GetByIdAsync(id);

            if (existing == null)
            {
                return null;
            }

            await EnsureCompoundExistsAsync(request.CompoundId);

            var oldValues = new
            {
                existing.Name,
                existing.CompoundId
            };

            var building = new BuildingEntity(0, request.Name, request.NumberOfApartments, request.Floors, request.CompoundId);

            BuildingEntity? updated = await _contextManager.UpdateAsync(id, building);

            if (updated != null)
            {
                await _auditService.LogAsync(LogAction.Update, "Building", id, oldValues, new { updated.Name, updated.CompoundId });

                return MapToResponse(updated);
            }

            return null;
        }

        public async Task<bool> DeleteBuildingAsync(int id)
        {
            BuildingEntity? existing = await _contextManager.GetByIdAsync(id);

            if (existing == null)
            {
                return false;
            }

            await _auditService.LogAsync(LogAction.Delete, "Building", id, new { existing.Name, existing.CompoundId }, null);

            return await _contextManager.DeleteAsync(id);
        }

        private async Task EnsureCompoundExistsAsync(int compoundId)
        {
            var compound = await _compoundContextManager.GetByIdAsync(compoundId);
            if (compound == null)
            {
                throw new ApiException(ErrorCodes.BUILDING_COMPOUND_INVALID_ERROR, "The requested compound does not exist", $"CompoundId: {compoundId}");
            }
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
