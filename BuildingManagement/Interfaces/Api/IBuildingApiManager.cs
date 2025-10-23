using BuildingManagement.Models.Api.Response.Building;
using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Api
{
    public interface IBuildingApiManager
    {
        Task<List<BuildingResponse>> GetAllBuildingsAsync();

        Task<BuildingResponse?> GetBuildingByIdAsync(int id);

        Task<List<BuildingResponse>> GetBuildingsByCompoundIdAsync(int compoundId);

        Task<BuildingResponse> CreateBuildingAsync(BuildingEntity building);

        Task<BuildingResponse?> UpdateBuildingAsync(int id, BuildingEntity building);

        Task<bool> DeleteBuildingAsync(int id);
    }
}