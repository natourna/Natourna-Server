using NatournaServer.Models.Api.Requests.Apartment;
using NatournaServer.Models.Api.Response.Apartment;
using NatournaServer.Models.Api.Response.Paging;

namespace NatournaServer.Interfaces.Api
{
    public interface IApartmentApiManager
    {
        Task<PagedResponse<ApartmentResponse>> GetApartmentsAsync(int page, int pageSize, int? buildingId, string? search);

        Task<ApartmentResponse?> GetApartmentByIdAsync(int id);

        Task<ApartmentResponse> CreateApartmentAsync(ApartmentRequest request);

        Task<ApartmentResponse?> UpdateApartmentAsync(int id, ApartmentRequest request);

        Task<bool> DeleteApartmentAsync(int id);

        Task<ApartmentResponse?> SetApartmentActiveAsync(int id, bool isActive);
    }
}
