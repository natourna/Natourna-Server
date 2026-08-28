using NatournaServer.Constants.User;
using NatournaServer.Interfaces.Api;
using NatournaServer.Models.Api.Requests.Compound;
using NatournaServer.Models.Api.Response.Compound;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NatournaServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CompoundController : ControllerBase
    {
        private readonly ICompoundApiManager _compoundManager;

        public CompoundController(ICompoundApiManager compoundManager)
        {
            _compoundManager = compoundManager;
        }

        [HttpGet]
        public async Task<ActionResult<List<CompoundResponse>>> GetAllCompounds()
        {
            var compounds = await _compoundManager.GetAllCompoundsAsync();
            return Ok(compounds);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CompoundResponse>> GetCompoundById(int id)
        {
            var compound = await _compoundManager.GetCompoundByIdAsync(id);

            if (compound == null)
            {
                return NotFound();
            }

            return Ok(compound);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<CompoundResponse>> CreateCompound([FromBody] CompoundRequest request)
        {
            var createdCompound = await _compoundManager.CreateCompoundAsync(request);
            return CreatedAtAction(nameof(GetCompoundById), new { id = createdCompound.Id }, createdCompound);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<CompoundResponse>> UpdateCompound(int id, [FromBody] CompoundRequest request)
        {
            var updatedCompound = await _compoundManager.UpdateCompoundAsync(id, request);

            if (updatedCompound == null)
            {
                return NotFound();
            }

            return Ok(updatedCompound);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult> DeleteCompound(int id)
        {
            var result = await _compoundManager.DeleteCompoundAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
