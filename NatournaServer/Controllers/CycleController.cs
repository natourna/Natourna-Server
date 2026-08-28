using NatournaServer.Constants.User;
using NatournaServer.Interfaces.Api;
using NatournaServer.Models.Api.Requests.Cycle;
using NatournaServer.Models.Api.Requests.Paging;
using NatournaServer.Models.Api.Response.Cycle;
using NatournaServer.Models.Api.Response.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NatournaServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = RoleNames.Admin)]
    public class CycleController : ControllerBase
    {
        private readonly ICycleApiManager _cycleApiManager;

        public CycleController(ICycleApiManager cycleApiManager)
        {
            _cycleApiManager = cycleApiManager;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<CycleResponse>>> GetCycles([FromQuery] PagedQuery paging)
        {
            var cycles = await _cycleApiManager.GetCyclesAsync(paging.Page, paging.PageSize);
            return Ok(cycles);
        }

        [HttpGet("active")]
        public async Task<ActionResult<CycleResponse>> GetActiveCycle()
        {
            CycleResponse? cycle = await _cycleApiManager.GetActiveCycleAsync();

            if (cycle == null)
            {
                return NotFound();
            }

            return Ok(cycle);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CycleResponse>> GetCycleById(int id)
        {
            CycleResponse? cycle = await _cycleApiManager.GetCycleByIdAsync(id);

            if (cycle == null)
            {
                return NotFound();
            }

            return Ok(cycle);
        }

        [HttpPost]
        public async Task<ActionResult<CycleResponse>> CreateCycle([FromBody] CycleRequest request)
        {
            CycleResponse createdCycle = await _cycleApiManager.CreateCycleAsync(request);

            return CreatedAtAction(nameof(GetCycleById), new { id = createdCycle.Id }, createdCycle);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CycleResponse>> UpdateCycle(int id, [FromBody] CycleUpdateRequest request)
        {
            CycleResponse? updatedCycle = await _cycleApiManager.UpdateCycleAsync(id, request);

            if (updatedCycle == null)
            {
                return NotFound();
            }

            return Ok(updatedCycle);
        }

        [HttpDelete("{id}")]
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
