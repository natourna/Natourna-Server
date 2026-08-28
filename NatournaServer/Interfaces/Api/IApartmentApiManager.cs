using NatournaServer.Models.Api.Response.Apartment;
using NatournaServer.Models.Api.Requests.Apartment;

namespace NatournaServer.Interfaces.Api
{
    public interface IApartmentApiManager
    {
        Task<List<ApartmentResponse>> GetAllApartmentsAsync();

        Task<ApartmentResponse?> GetApartmentByIdAsync(int id);

        Task<List<ApartmentResponse>> GetApartmentsByBuildingIdAsync(int buildingId);

        Task<ApartmentResponse> CreateApartmentAsync(ApartmentRequest apartment);

        Task<ApartmentResponse?> UpdateApartmentAsync(int id, ApartmentRequest apartment);

        Task<bool> DeleteApartmentAsync(int id);

        Task<ApartmentResponse?> SetApartmentActiveAsync(int id, bool isActive);
    }
}