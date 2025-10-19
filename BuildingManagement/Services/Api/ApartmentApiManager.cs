using BuildingManagement.Constants.Log;
using BuildingManagement.Interfaces.Api;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Interfaces.Services;
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

        public async Task<List<ApartmentEntity>> GetAllApartmentsAsync()
        {
            return await _contextManager.GetAllAsync();
        }

        public async Task<ApartmentEntity?> GetApartmentByIdAsync(int id)
        {
            return await _contextManager.GetByIdAsync(id);
        }

        public async Task<List<ApartmentEntity>> GetApartmentsByBuildingIdAsync(int buildingId)
        {
            return await _contextManager.GetByBuildingIdAsync(buildingId);
        }

        public async Task<ApartmentEntity> CreateApartmentAsync(ApartmentEntity apartment)
        {
            var created = await _contextManager.CreateAsync(apartment);

            await _auditService.LogAsync(LogAction.Create, "Apartment", created.Id, null, new
            {
                created.BuildingId,
                created.ApartmentInfo,
                created.IsActive
            });

            return created;
        }

        public async Task<ApartmentEntity?> UpdateApartmentAsync(int id, ApartmentEntity apartment)
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
            }

            return updated;
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

        public async Task<ApartmentEntity?> SetApartmentActiveAsync(int id, bool isActive)
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
            }

            return result;
        }
    }
}