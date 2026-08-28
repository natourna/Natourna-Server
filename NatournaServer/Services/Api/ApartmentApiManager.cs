using NatournaServer.Constants.Error;
using NatournaServer.Constants.Log;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Models.Api.Requests.Apartment;
using NatournaServer.Models.Api.Response.Apartment;
using NatournaServer.Models.Api.Response.Paging;
using NatournaServer.Models.Entities;

namespace NatournaServer.Services.Api
{
    public class ApartmentApiManager : IApartmentApiManager
    {
        private readonly IApartmentContextManager _contextManager;
        private readonly IBuildingContextManager _buildingContextManager;
        private readonly IAuditService _auditService;

        public ApartmentApiManager(IApartmentContextManager contextManager, IBuildingContextManager buildingContextManager, IAuditService auditService)
        {
            _contextManager = contextManager;
            _buildingContextManager = buildingContextManager;
            _auditService = auditService;
        }

        public async Task<PagedResponse<ApartmentResponse>> GetApartmentsAsync(int page, int pageSize, int? buildingId, string? search)
        {
            (List<ApartmentEntity> items, int totalCount) = await _contextManager.GetPagedAsync(page, pageSize, buildingId, search);

            return new PagedResponse<ApartmentResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<ApartmentResponse?> GetApartmentByIdAsync(int id)
        {
            ApartmentEntity? apartment = await _contextManager.GetByIdAsync(id);
            return apartment == null ? null : MapToResponse(apartment);
        }

        public async Task<ApartmentResponse> CreateApartmentAsync(ApartmentRequest request)
        {
            await EnsureBuildingExistsAsync(request.BuildingId);

            var apartment = new ApartmentEntity(request.ApartmentInfo, request.Floor, request.IsActive, request.BuildingId)
            {
                Owner = request.Owner,
                Tenant = request.Tenant
            };

            ApartmentEntity created = await _contextManager.CreateAsync(apartment);

            await _auditService.LogAsync(LogAction.Create, "Apartment", created.Id, null, new { created.BuildingId, created.ApartmentInfo, created.IsActive });

            return MapToResponse(created);
        }

        public async Task<ApartmentResponse?> UpdateApartmentAsync(int id, ApartmentRequest request)
        {
            ApartmentEntity? existing = await _contextManager.GetByIdAsync(id);

            if (existing == null)
            {
                return null;
            }

            await EnsureBuildingExistsAsync(request.BuildingId);

            var oldValues = new
            {
                existing.BuildingId,
                existing.ApartmentInfo,
                existing.IsActive
            };

            var apartment = new ApartmentEntity(request.ApartmentInfo, request.Floor, request.IsActive, request.BuildingId)
            {
                Owner = request.Owner,
                Tenant = request.Tenant
            };

            ApartmentEntity? updated = await _contextManager.UpdateAsync(id, apartment);

            if (updated != null)
            {
                await _auditService.LogAsync(LogAction.Update, "Apartment", id, oldValues, new { updated.BuildingId, updated.ApartmentInfo, updated.IsActive });

                return MapToResponse(updated);
            }

            return null;
        }

        public async Task<bool> DeleteApartmentAsync(int id)
        {
            ApartmentEntity? existing = await _contextManager.GetByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            await _auditService.LogAsync(LogAction.Delete, "Apartment", id, new { existing.BuildingId, existing.ApartmentInfo }, null);

            return await _contextManager.DeleteAsync(id);
        }

        public async Task<ApartmentResponse?> SetApartmentActiveAsync(int id, bool isActive)
        {
            ApartmentEntity? existing = await _contextManager.GetByIdAsync(id);

            if (existing == null)
            {
                return null;
            }

            ApartmentEntity? result = await _contextManager.SetActiveAsync(id, isActive);

            if (result != null)
            {
                LogAction action = isActive ? LogAction.ActivateUser : LogAction.DeactivateUser;
                await _auditService.LogAsync(action, "Apartment", id, new { existing.IsActive }, new { IsActive = isActive });

                return MapToResponse(result);
            }

            return null;
        }

        private async Task EnsureBuildingExistsAsync(int buildingId)
        {
            var building = await _buildingContextManager.GetByIdAsync(buildingId);
            if (building == null)
            {
                throw new ApiException(ErrorCodes.APARTMENT_BUILDING_INVALID_ERROR, "The requested building does not exist", $"BuildingId: {buildingId}");
            }
        }

        private static ApartmentResponse MapToResponse(ApartmentEntity apartment)
        {
            return new ApartmentResponse
            {
                Id = apartment.Id,
                ApartmentInfo = apartment.ApartmentInfo,
                Owner = apartment.Owner,
                Tenant = apartment.Tenant,
                IsActive = apartment.IsActive,
                Floor = apartment.Floor,
                BuildingId = apartment.BuildingId,
                BuildingName = apartment.Building?.Name,
                CreatedAt = apartment.CreatedAt,
                UpdatedAt = apartment.UpdatedAt
            };
        }
    }
}
