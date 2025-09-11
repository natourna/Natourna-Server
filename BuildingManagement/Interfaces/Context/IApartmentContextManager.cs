using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Context
{
    public interface IApartmentContextManager
    {
        Task<List<ApartmentEntity>> GetAllAsync();

        Task<ApartmentEntity?> GetByIdAsync(int id);

        Task<List<ApartmentEntity>> GetByBuildingIdAsync(int buildingId);

        Task<ApartmentEntity> CreateAsync(ApartmentEntity apartment);

        Task<ApartmentEntity?> UpdateAsync(int id, ApartmentEntity apartment);

        Task<bool> DeleteAsync(int id);
    }
}