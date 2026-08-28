using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Context
{
    public interface ICompoundContextManager
    {
        Task<List<CompoundEntity>> GetAllAsync();

        Task<CompoundEntity?> GetByIdAsync(int id);

        Task<CompoundEntity> CreateAsync(CompoundEntity compound);

        Task<CompoundEntity?> UpdateAsync(int id, CompoundEntity compound);

        Task<bool> DeleteAsync(int id);
    }
}