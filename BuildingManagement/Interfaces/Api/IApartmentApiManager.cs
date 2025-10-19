using BuildingManagement.Models.Api.Response.Apartment;
using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Api
{
    public interface IApartmentApiManager
    {
        Task<List<ApartmentResponse>> GetAllApartmentsAsync();

        Task<ApartmentResponse?> GetApartmentByIdAsync(int id);

        Task<List<ApartmentResponse>> GetApartmentsByBuildingIdAsync(int buildingId);

        Task<ApartmentResponse> CreateApartmentAsync(ApartmentEntity apartment);

        Task<ApartmentResponse?> UpdateApartmentAsync(int id, ApartmentEntity apartment);

        Task<bool> DeleteApartmentAsync(int id);

        Task<ApartmentResponse?> SetApartmentActiveAsync(int id, bool isActive);
    }
}