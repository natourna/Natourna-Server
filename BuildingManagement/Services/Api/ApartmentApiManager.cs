using BuildingManagement.Constants.Log;
using BuildingManagement.Interfaces.Api;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Interfaces.Services;
using BuildingManagement.Models.Api.Response.Apartment;
using BuildingManagement.Models.Entities;

namespace BuildingManagement.Services.Api
{
    public class ApartmentApiManager : IApartmentApiManager
    {
        private readonly IApartmentContextManager _contextManager;
        private readonly IAuditService _auditService;

        public ApartmentApiManager(IApartmentContextManager contextManager, IAuditService auditService)
        {
            _contextManager = contextManager;
            _auditService = auditService;
        }

        public async Task<List<ApartmentResponse>> GetAllApartmentsAsync()
        {
            var apartments = await _contextManager.GetAllAsync();
            return apartments.Select(MapToResponse).ToList();
        }

        public async Task<ApartmentResponse?> GetApartmentByIdAsync(int id)
        {
            var apartment = await _contextManager.GetByIdAsync(id);
            return apartment == null ? null : MapToResponse(apartment);
        }

        public async Task<List<ApartmentResponse>> GetApartmentsByBuildingIdAsync(int buildingId)
        {
            var apartments = await _contextManager.GetByBuildingIdAsync(buildingId);
            return apartments.Select(MapToResponse).ToList();
        }

        public async Task<ApartmentResponse> CreateApartmentAsync(ApartmentEntity apartment)
        {
            var created = await _contextManager.CreateAsync(apartment);

            await _auditService.LogAsync(LogAction.Create, "Apartment", created.Id, null, new
            {
                created.BuildingId,
                created.ApartmentInfo,
                created.IsActive
            });

            return MapToResponse(created);
        }

        public async Task<ApartmentResponse?> UpdateApartmentAsync(int id, ApartmentEntity apartment)
        {
            var existing = await _contextManager.GetByIdAsync(id);
            if (existing == null)
            {
                return null;
            }

            var oldValues = new
            {
                existing.BuildingId,
                existing.ApartmentInfo,
                existing.IsActive
            };

            var updated = await _contextManager.UpdateAsync(id, apartment);

            if (updated != null)
            {
                await _auditService.LogAsync(LogAction.Update, "Apartment", id, oldValues, new
                {
                    updated.BuildingId,
                    updated.ApartmentInfo,
                    updated.IsActive
                });

                return MapToResponse(updated);
            }

            return null;
        }

        public async Task<bool> DeleteApartmentAsync(int id)
        {
            var existing = await _contextManager.GetByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            await _auditService.LogAsync(LogAction.Delete, "Apartment", id, new
            {
                existing.BuildingId,
                existing.ApartmentInfo
            }, null);

            return await _contextManager.DeleteAsync(id);
        }

        public async Task<ApartmentResponse?> SetApartmentActiveAsync(int id, bool isActive)
        {
            var existing = await _contextManager.GetByIdAsync(id);
            if (existing == null)
            {
                return null;
            }

            var result = await _contextManager.SetActiveAsync(id, isActive);

            if (result != null)
            {
                var action = isActive ? LogAction.ActivateUser : LogAction.DeactivateUser;
                await _auditService.LogAsync(action, "Apartment", id,
                    new { IsActive = existing.IsActive },
                    new { IsActive = isActive });

                return MapToResponse(result);
            }

            return null;
        }

        /// <summary>
        /// Maps ApartmentEntity to ApartmentResponse DTO
        /// </summary>
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