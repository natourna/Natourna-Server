using NatrounaServer.Models.Entities;

namespace NatrounaServer.Interfaces.Context
{
    public interface IBuildingContextManager
    {
        Task<List<BuildingEntity>> GetAllAsync();

        Task<BuildingEntity?> GetByIdAsync(int id);

        Task<List<BuildingEntity>> GetByCompoundIdAsync(int compoundId);

        Task<BuildingEntity> CreateAsync(BuildingEntity building);

        Task<BuildingEntity?> UpdateAsync(int id, BuildingEntity building);

        Task<bool> DeleteAsync(int id);
    }
}