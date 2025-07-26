using BuildingManagement.Interfaces.Api;
using BuildingManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BuildingManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompoundController : ControllerBase
    {
        private readonly ICompoundApiManager _compoundManager;

        public CompoundController(ICompoundApiManager compoundManager)
        {
            _compoundManager = compoundManager;
        }

        [HttpGet]
        public async Task<ActionResult<List<CompoundEntity>>> GetAllCompounds()
        {
            var compounds = await _compoundManager.GetAllCompoundsAsync();

            return Ok(compounds);
        }

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

        [HttpPost]
        public async Task<ActionResult<CompoundEntity>> CreateCompound(CompoundEntity compound)
        {
            var createdCompound = await _compoundManager.CreateCompoundAsync(compound);

            return CreatedAtAction(nameof(GetCompoundById), new { id = createdCompound.Id }, createdCompound);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CompoundEntity>> UpdateCompound(int id, CompoundEntity compound)
        {
            var updatedCompound = await _compoundManager.UpdateCompoundAsync(id, compound);

            if (updatedCompound == null)
            {
                return NotFound();
            }

            return Ok(updatedCompound);
        }

        [HttpDelete("{id}")]
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