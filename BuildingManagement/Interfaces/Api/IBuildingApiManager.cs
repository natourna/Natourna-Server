using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Api
{
    public interface IBuildingApiManager
    {
        Task<List<BuildingEntity>> GetAllBuildingsAsync();

        Task<BuildingEntity?> GetBuildingByIdAsync(int id);

        Task<List<BuildingEntity>> GetBuildingsByCompoundIdAsync(int compoundId);

        Task<BuildingEntity> CreateBuildingAsync(BuildingEntity building);

        Task<BuildingEntity?> UpdateBuildingAsync(int id, BuildingEntity building);

        Task<bool> DeleteBuildingAsync(int id);
    }
}