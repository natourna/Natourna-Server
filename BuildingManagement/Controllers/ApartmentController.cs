using BuildingManagement.Interfaces.Api;
using BuildingManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildingManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApartmentController : ControllerBase
    {
        private readonly IApartmentApiManager _apartmentManager;

        public ApartmentController(IApartmentApiManager apartmentManager)
        {
            _apartmentManager = apartmentManager;
        }

        [HttpGet]
        public async Task<ActionResult<List<ApartmentEntity>>> GetAllApartments()
        {
            var apartments = await _apartmentManager.GetAllApartmentsAsync();

            return Ok(apartments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApartmentEntity>> GetApartmentById(int id)
        {
            var apartment = await _apartmentManager.GetApartmentByIdAsync(id);

            if (apartment == null)
            {
                return NotFound();
            }

            return Ok(apartment);
        }

        [HttpGet("building/{buildingId}")]
        public async Task<ActionResult<List<ApartmentEntity>>> GetApartmentsByBuildingId(int buildingId)
        {
            var apartments = await _apartmentManager.GetApartmentsByBuildingIdAsync(buildingId);

            return Ok(apartments);
        }

        [HttpPost]
        public async Task<ActionResult<ApartmentEntity>> CreateApartment(ApartmentEntity apartment)
        {
            var createdApartment = await _apartmentManager.CreateApartmentAsync(apartment);

            return CreatedAtAction(nameof(GetApartmentById), new { id = createdApartment.Id }, createdApartment);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApartmentEntity>> UpdateApartment(int id, ApartmentEntity apartment)
        {
            var updatedApartment = await _apartmentManager.UpdateApartmentAsync(id, apartment);

            if (updatedApartment == null)
            {
                return NotFound();
            }

            return Ok(updatedApartment);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteApartment(int id)
        {
            var result = await _apartmentManager.DeleteApartmentAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}