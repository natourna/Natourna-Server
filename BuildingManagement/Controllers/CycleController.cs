using BuildingManagement.Interfaces.Api;
using BuildingManagement.Models.Api.Requests.Cycle;
using BuildingManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildingManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CycleController : ControllerBase
    {
        private readonly ICycleApiManager _cycleApiManager;

        public CycleController(ICycleApiManager cycleApiManager)
        {
            _cycleApiManager = cycleApiManager;
        }

        /// <summary>
        /// Get all cycles - Any authenticated user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<CycleEntity>>> GetAllCycles()
        {
            var cycles = await _cycleApiManager.GetAllCyclesAsync();
            return Ok(cycles);
        }

        /// <summary>
        /// Get cycle by ID - Any authenticated user
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CycleEntity>> GetCycleById(int id)
        {
            var cycle = await _cycleApiManager.GetCycleByIdAsync(id);
            if (cycle == null)
            {
                return NotFound();
            }

            return Ok(cycle);
        }

        /// <summary>
        /// Create cycle - Admin only
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CycleEntity>> CreateCycle([FromBody] CycleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdCycle = await _cycleApiManager.CreateCycleAsync(request);
            return CreatedAtAction(nameof(GetCycleById), new { id = createdCycle.Id }, createdCycle);
        }

        /// <summary>
        /// Update cycle - Admin only
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CycleEntity>> UpdateCycle(int id, CycleEntity cycle)
        {
            var updatedCycle = await _cycleApiManager.UpdateCycleAsync(id, cycle);
            if (updatedCycle == null)
            {
                return NotFound();
            }

            return Ok(updatedCycle);
        }

        /// <summary>
        /// Delete cycle - Admin only
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCycle(int id)
        {
            var result = await _cycleApiManager.DeleteCycleAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}