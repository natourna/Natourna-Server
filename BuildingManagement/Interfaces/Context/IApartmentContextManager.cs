using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Context
{
    public interface IApartmentContextManager
    {
        Task<List<ApartementEntity>> GetAllAsync();

        Task<ApartementEntity?> GetByIdAsync(int id);

        Task<List<ApartementEntity>> GetByBuildingIdAsync(int buildingId);

        Task<List<ApartementEntity>> GetByUserIdAsync(int userId);

        Task<ApartementEntity> CreateAsync(ApartementEntity apartment);

        Task<ApartementEntity?> UpdateAsync(int id, ApartementEntity apartment);

        Task<bool> DeleteAsync(int id);
    }
}