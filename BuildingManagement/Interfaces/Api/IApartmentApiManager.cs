using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Api
{
    public interface IApartmentApiManager
    {
        Task<List<ApartmentEntity>> GetAllApartmentsAsync();

        Task<ApartmentEntity?> GetApartmentByIdAsync(int id);

        Task<List<ApartmentEntity>> GetApartmentsByBuildingIdAsync(int buildingId);

        Task<ApartmentEntity> CreateApartmentAsync(ApartmentEntity apartment);

        Task<ApartmentEntity?> UpdateApartmentAsync(int id, ApartmentEntity apartment);

        Task<bool> DeleteApartmentAsync(int id);

        Task<ApartmentEntity?> SetApartmentActiveAsync(int id, bool isActive);
    }
}