using BuildingManagement.Interfaces.Api;
using BuildingManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BuildingManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApartmentController : ControllerBase
    {
        private readonly IApartmentApiManager _apartmentManager;

        public ApartmentController(IApartmentApiManager apartmentManager)
        {
            _apartmentManager = apartmentManager;
        }

        [HttpGet]
        public async Task<ActionResult<List<ApartementEntity>>> GetAllApartments()
        {
            var apartments = await _apartmentManager.GetAllApartmentsAsync();

            return Ok(apartments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApartementEntity>> GetApartmentById(int id)
        {
            var apartment = await _apartmentManager.GetApartmentByIdAsync(id);

            if (apartment == null)
            {
                return NotFound();
            }

            return Ok(apartment);
        }

        [HttpGet("building/{buildingId}")]
        public async Task<ActionResult<List<ApartementEntity>>> GetApartmentsByBuildingId(int buildingId)
        {
            var apartments = await _apartmentManager.GetApartmentsByBuildingIdAsync(buildingId);

            return Ok(apartments);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<ApartementEntity>>> GetApartmentsByUserId(int userId)
        {
            var apartments = await _apartmentManager.GetApartmentsByUserIdAsync(userId);

            return Ok(apartments);
        }

        [HttpPost]
        public async Task<ActionResult<ApartementEntity>> CreateApartment(ApartementEntity apartment)
        {
            var createdApartment = await _apartmentManager.CreateApartmentAsync(apartment);

            return CreatedAtAction(nameof(GetApartmentById), new { id = createdApartment.Id }, createdApartment);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApartementEntity>> UpdateApartment(int id, ApartementEntity apartment)
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