using BuildingManagement.Models.Entities;
using BuildingManagement.Models.Requests.Cycle;

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
