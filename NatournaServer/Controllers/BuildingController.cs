using NatournaServer.Constants.User;
using NatournaServer.Interfaces.Api;
using NatournaServer.Models.Api.Requests.Building;
using NatournaServer.Models.Api.Response.Building;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NatournaServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BuildingController : ControllerBase
    {
        private readonly IBuildingApiManager _buildingManager;

        public BuildingController(IBuildingApiManager buildingManager)
        {
            _buildingManager = buildingManager;
        }

        [HttpGet]
        public async Task<ActionResult<List<BuildingResponse>>> GetAllBuildings()
        {
            var buildings = await _buildingManager.GetAllBuildingsAsync();
            return Ok(buildings);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BuildingResponse>> GetBuildingById(int id)
        {
            var building = await _buildingManager.GetBuildingByIdAsync(id);

            if (building == null)
            {
                return NotFound();
            }

            return Ok(building);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<BuildingResponse>> CreateBuilding([FromBody] BuildingRequest request)
        {
            var createdBuilding = await _buildingManager.CreateBuildingAsync(request);
            return CreatedAtAction(nameof(GetBuildingById), new { id = createdBuilding.Id }, createdBuilding);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<BuildingResponse>> UpdateBuilding(int id, [FromBody] BuildingRequest request)
        {
            var updatedBuilding = await _buildingManager.UpdateBuildingAsync(id, request);

            if (updatedBuilding == null)
            {
                return NotFound();
            }

            return Ok(updatedBuilding);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult> DeleteBuilding(int id)
        {
            var result = await _buildingManager.DeleteBuildingAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
