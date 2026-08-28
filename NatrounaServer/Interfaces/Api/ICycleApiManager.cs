using NatrounaServer.Models.Api.Requests.Cycle;
using NatrounaServer.Models.Api.Response.Cycle;
using NatrounaServer.Models.Entities;

namespace NatrounaServer.Interfaces.Api
{
    public interface ICycleApiManager
    {
        Task<List<CycleResponse>> GetAllCyclesAsync();

        Task<CycleResponse?> GetCycleByIdAsync(int id);

        Task<CycleEntity> CreateCycleAsync(CycleRequest request);

        Task<CycleEntity?> UpdateCycleAsync(int id, CycleEntity cycle);

        Task<bool> DeleteCycleAsync(int id);
    }
}
