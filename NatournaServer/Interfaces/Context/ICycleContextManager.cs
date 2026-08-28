using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Context
{
    public interface ICycleContextManager
    {
        Task<(List<CycleEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);

        Task<CycleEntity?> GetActiveAsync();

        Task<CycleEntity?> GetByIdAsync(int id);

        Task<CycleEntity> CreateAsync(CycleEntity cycle);

        Task<CycleEntity?> UpdateAsync(int id, string label, string? description, bool isActive);

        Task<bool> DeleteAsync(int id);
    }
}
