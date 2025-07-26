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

        public async Task<List<ApartementEntity>> GetAllApartmentsAsync()
        {
            return await _contextManager.GetAllAsync();
        }

        public async Task<ApartementEntity?> GetApartmentByIdAsync(int id)
        {
            return await _contextManager.GetByIdAsync(id);
        }

        public async Task<List<ApartementEntity>> GetApartmentsByBuildingIdAsync(int buildingId)
        {
            return await _contextManager.GetByBuildingIdAsync(buildingId);
        }

        public async Task<List<ApartementEntity>> GetApartmentsByUserIdAsync(int userId)
        {
            return await _contextManager.GetByUserIdAsync(userId);
        }

        public async Task<ApartementEntity> CreateApartmentAsync(ApartementEntity apartment)
        {
            return await _contextManager.CreateAsync(apartment);
        }

        public async Task<ApartementEntity?> UpdateApartmentAsync(int id, ApartementEntity apartment)
        {
            return await _contextManager.UpdateAsync(id, apartment);
        }

        public async Task<bool> DeleteApartmentAsync(int id)
        {
            return await _contextManager.DeleteAsync(id);
        }
    }
}