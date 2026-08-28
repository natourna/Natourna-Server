using NatournaServer.Models.Api.Response.Building;
using NatournaServer.Models.Api.Requests.Building;

namespace NatournaServer.Interfaces.Api
{
    public interface IBuildingApiManager
    {
        Task<List<BuildingResponse>> GetAllBuildingsAsync();

        Task<BuildingResponse?> GetBuildingByIdAsync(int id);

        Task<List<BuildingResponse>> GetBuildingsByCompoundIdAsync(int compoundId);

        Task<BuildingResponse> CreateBuildingAsync(BuildingRequest building);

        Task<BuildingResponse?> UpdateBuildingAsync(int id, BuildingRequest building);

        Task<bool> DeleteBuildingAsync(int id);
    }
}