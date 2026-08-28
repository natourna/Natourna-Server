using NatournaServer.Constants.User;
using NatournaServer.Interfaces.Api;
using NatournaServer.Models.Entities;
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

        /// <summary>
        /// Get all compounds - Any authenticated user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<CompoundEntity>>> GetAllCompounds()
        {
            var compounds = await _compoundManager.GetAllCompoundsAsync();
            return Ok(compounds);
        }

        /// <summary>
        /// Get compound by ID - Any authenticated user
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CompoundEntity>> GetCompoundById(int id)
        {
            var compound = await _compoundManager.GetCompoundByIdAsync(id);

            if (compound == null)
            {
                return NotFound();
            }

            return Ok(compound);
        }

        /// <summary>
        /// Create compound - Admin only
        /// </summary>
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<CompoundEntity>> CreateCompound(CompoundEntity compound)
        {
            var createdCompound = await _compoundManager.CreateCompoundAsync(compound);
            return CreatedAtAction(nameof(GetCompoundById), new { id = createdCompound.Id }, createdCompound);
        }

        /// <summary>
        /// Update compound - Admin only
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<CompoundEntity>> UpdateCompound(int id, CompoundEntity compound)
        {
            var updatedCompound = await _compoundManager.UpdateCompoundAsync(id, compound);

            if (updatedCompound == null)
            {
                return NotFound();
            }

            return Ok(updatedCompound);
        }

        /// <summary>
        /// Delete compound - Admin only
        /// </summary>
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