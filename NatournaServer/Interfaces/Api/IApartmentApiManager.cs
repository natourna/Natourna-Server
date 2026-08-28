using NatournaServer.Models.Api.Response.Apartment;
using NatournaServer.Models.Api.Requests.Apartment;
using NatournaServer.Models.Api.Requests.Paging;
using NatournaServer.Models.Api.Response.Paging;

namespace NatournaServer.Interfaces.Api
{
    public interface IApartmentApiManager
    {
        Task<PagedResponse<ApartmentResponse>> GetPagedApartmentsAsync(PagedQuery query, int? buildingId = null, bool? isActive = null, string? search = null);

        Task<ApartmentResponse?> GetApartmentByIdAsync(int id);

        Task<List<ApartmentResponse>> GetApartmentsByBuildingIdAsync(int buildingId);

        Task<ApartmentResponse> CreateApartmentAsync(ApartmentRequest apartment);

        Task<ApartmentResponse?> UpdateApartmentAsync(int id, ApartmentRequest apartment);

        Task<bool> DeleteApartmentAsync(int id);

        Task<ApartmentResponse?> SetApartmentActiveAsync(int id, bool isActive);
    }
}