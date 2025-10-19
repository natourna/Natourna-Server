using BuildingManagement.Models.Api.Requests.Cycle;
using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Api
{
    public interface ICycleApiManager
    {
        Task<List<CycleEntity>> GetAllCyclesAsync();
        Task<CycleEntity?> GetCycleByIdAsync(int id);
        Task<CycleEntity> CreateCycleAsync(CycleRequest request);
        Task<CycleEntity?> UpdateCycleAsync(int id, CycleEntity cycle);
        Task<bool> DeleteCycleAsync(int id);
    }
}
