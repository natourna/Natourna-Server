using BuildingManagement.Interfaces.Api;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Models.Entities;

namespace BuildingManagement.Services.Api
{
    public class ApartmentApiManager : IApartmentApiManager
    {
        private readonly IApartmentContextManager _contextManager;

        public ApartmentApiManager(IApartmentContextManager contextManager)
        {
            _contextManager = contextManager;
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

        public async Task<List<ApartmentEntity>> GetApartmentsByUserIdAsync(int userId)
        {
            return await _contextManager.GetByUserIdAsync(userId);
        }

        public async Task<ApartmentEntity> CreateApartmentAsync(ApartmentEntity apartment)
        {
            return await _contextManager.CreateAsync(apartment);
        }

        public async Task<ApartmentEntity?> UpdateApartmentAsync(int id, ApartmentEntity apartment)
        {
            return await _contextManager.UpdateAsync(id, apartment);
        }

        public async Task<bool> DeleteApartmentAsync(int id)
        {
            return await _contextManager.DeleteAsync(id);
        }
    }
}