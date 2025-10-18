using BuildingManagement.Interfaces.Api;
using BuildingManagement.Models.Entities;
using BuildingManagement.Models.Requests.Cycle;
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
        /// Get all payment cycles
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<CycleEntity>>> GetAllCycles()
        {
            var cycles = await _cycleApiManager.GetAllCyclesAsync();
            return Ok(cycles);
        }

        /// <summary>
        /// Get a specific cycle by ID (includes all generated payments)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CycleEntity>> GetCycleById(int id)
        {
            var cycle = await _cycleApiManager.GetCycleByIdAsync(id);
            if (cycle == null)
                return NotFound();

            return Ok(cycle);
        }

        /// <summary>
        /// Create a new payment cycle.
        /// This will automatically generate payments for all specified apartments (or all active apartments if none specified)
        /// for all occurrences within the date range based on the cycle type.
        /// 
        /// Example: Monthly cycle from Jan 1 to Dec 31 = 12 payments per apartment
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CycleEntity>> CreateCycle([FromBody] CycleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdCycle = await _cycleApiManager.CreateCycleAsync(request);
            return CreatedAtAction(nameof(GetCycleById), new { id = createdCycle.Id }, createdCycle);
        }

        /// <summary>
        /// Update an existing cycle
        /// Note: This updates cycle metadata only, not the generated payments
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<CycleEntity>> UpdateCycle(int id, CycleEntity cycle)
        {
            var updatedCycle = await _cycleApiManager.UpdateCycleAsync(id, cycle);
            if (updatedCycle == null)
                return NotFound();

            return Ok(updatedCycle);
        }

        /// <summary>
        /// Delete a cycle and all its associated payments
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCycle(int id)
        {
            var result = await _cycleApiManager.DeleteCycleAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}