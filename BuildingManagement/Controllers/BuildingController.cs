using BuildingManagement.Interfaces.Api;
using BuildingManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildingManagement.Controllers
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
        public async Task<ActionResult<List<BuildingEntity>>> GetAllBuildings()
        {
            var buildings = await _buildingManager.GetAllBuildingsAsync();

            return Ok(buildings);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BuildingEntity>> GetBuildingById(int id)
        {
            var building = await _buildingManager.GetBuildingByIdAsync(id);

            if (building == null)
            {
                return NotFound();
            }

            return Ok(building);
        }

        [HttpGet("compound/{compoundId}")]
        public async Task<ActionResult<List<BuildingEntity>>> GetBuildingsByCompoundId(int compoundId)
        {
            var buildings = await _buildingManager.GetBuildingsByCompoundIdAsync(compoundId);

            return Ok(buildings);
        }

        [HttpPost]
        public async Task<ActionResult<BuildingEntity>> CreateBuilding(BuildingEntity building)
        {
            var createdBuilding = await _buildingManager.CreateBuildingAsync(building);

            return CreatedAtAction(nameof(GetBuildingById), new { id = createdBuilding.Id }, createdBuilding);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BuildingEntity>> UpdateBuilding(int id, BuildingEntity building)
        {
            var updatedBuilding = await _buildingManager.UpdateBuildingAsync(id, building);

            if (updatedBuilding == null)
            {
                return NotFound();
            }

            return Ok(updatedBuilding);
        }

        [HttpDelete("{id}")]
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