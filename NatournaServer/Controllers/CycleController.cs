using NatournaServer.Constants.User;
using NatournaServer.Interfaces.Api;
using NatournaServer.Models.Api.Requests.Cycle;
using NatournaServer.Models.Api.Response.Cycle;
using NatournaServer.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NatournaServer.Controllers
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
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<List<CycleResponse>>> GetAllCycles()
        {
            List<CycleResponse> cycles = await _cycleApiManager.GetAllCyclesAsync();
            return Ok(cycles);
        }

        /// <summary>
        /// Get cycle by ID - Any authenticated user
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<CycleResponse?>> GetCycleById(int id)
        {
            CycleResponse? cycle = await _cycleApiManager.GetCycleByIdAsync(id);

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
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<CycleResponse>> CreateCycle([FromBody] CycleRequest request)
        {
            CycleResponse createdCycle = await _cycleApiManager.CreateCycleAsync(request);

            return CreatedAtAction(nameof(GetCycleById), new { id = createdCycle.Id }, createdCycle);
        }

        /// <summary>
        /// Update cycle - Admin only
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<CycleResponse>> UpdateCycle(int id, CycleUpdateRequest cycle)
        {
            CycleResponse? updatedCycle = await _cycleApiManager.UpdateCycleAsync(id, cycle);

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
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult> DeleteCycle(int id)
        {
            bool result = await _cycleApiManager.DeleteCycleAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}