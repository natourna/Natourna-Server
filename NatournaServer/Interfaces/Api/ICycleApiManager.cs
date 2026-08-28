using NatournaServer.Models.Api.Requests.Cycle;
using NatournaServer.Models.Api.Response.Cycle;
using NatournaServer.Models.Api.Response.Paging;

namespace NatournaServer.Interfaces.Api
{
    public interface ICycleApiManager
    {
        Task<PagedResponse<CycleResponse>> GetCyclesAsync(int page, int pageSize);

        Task<CycleResponse?> GetActiveCycleAsync();

        Task<CycleResponse?> GetCycleByIdAsync(int id);

        Task<CycleResponse> CreateCycleAsync(CycleRequest request);

        Task<CycleResponse?> UpdateCycleAsync(int id, CycleUpdateRequest request);

        Task<bool> DeleteCycleAsync(int id);
    }
}
