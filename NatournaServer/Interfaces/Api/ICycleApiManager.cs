using NatournaServer.Models.Api.Requests.Cycle;
using NatournaServer.Models.Api.Response.Cycle;
using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Api
{
    public interface ICycleApiManager
    {
        Task<List<CycleResponse>> GetAllCyclesAsync();

        Task<CycleResponse?> GetCycleByIdAsync(int id);

        Task<CycleEntity> CreateCycleAsync(CycleRequest request);

        Task<CycleEntity?> UpdateCycleAsync(int id, CycleUpdateRequest cycle);

        Task<bool> DeleteCycleAsync(int id);
    }
}
