using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Context
{
    public interface IBuildingContextManager
    {
        Task<List<BuildingEntity>> GetAllAsync();

        Task<BuildingEntity?> GetByIdAsync(int id);

        Task<BuildingEntity> CreateAsync(BuildingEntity building);

        Task<BuildingEntity?> UpdateAsync(int id, BuildingEntity building);

        Task<bool> DeleteAsync(int id);
    }
}
