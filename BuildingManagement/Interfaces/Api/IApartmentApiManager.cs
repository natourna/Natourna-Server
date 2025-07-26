using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Api
{
    public interface IApartmentApiManager
    {
        Task<List<ApartementEntity>> GetAllApartmentsAsync();

        Task<ApartementEntity?> GetApartmentByIdAsync(int id);

        Task<List<ApartementEntity>> GetApartmentsByBuildingIdAsync(int buildingId);

        Task<List<ApartementEntity>> GetApartmentsByUserIdAsync(int userId);

        Task<ApartementEntity> CreateApartmentAsync(ApartementEntity apartment);

        Task<ApartementEntity?> UpdateApartmentAsync(int id, ApartementEntity apartment);

        Task<bool> DeleteApartmentAsync(int id);
    }
}