using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Context
{
    public interface ICycleContextManager
    {
        Task<List<CycleEntity>> GetAllAsync(int? cycleId = null, bool? isActive = null);

        Task<CycleEntity?> GetByIdAsync(int id);

        Task<CycleEntity> CreateAsync(CycleEntity cycle);

        Task<CycleEntity?> UpdateAsync(int id, CycleEntity cycle);

        Task<bool> DeleteAsync(int id);
    }
}