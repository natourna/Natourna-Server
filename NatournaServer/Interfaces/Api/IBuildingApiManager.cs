using NatournaServer.Models.Api.Requests.Building;
using NatournaServer.Models.Api.Response.Building;

namespace NatournaServer.Interfaces.Api
{
    public interface IBuildingApiManager
    {
        Task<List<BuildingResponse>> GetAllBuildingsAsync();

        Task<BuildingResponse?> GetBuildingByIdAsync(int id);

        Task<BuildingResponse> CreateBuildingAsync(BuildingRequest request);

        Task<BuildingResponse?> UpdateBuildingAsync(int id, BuildingRequest request);

        Task<bool> DeleteBuildingAsync(int id);
    }
}
