using NatournaServer.Constants.User;
using NatournaServer.Interfaces.Api;
using NatournaServer.Models.Api.Response.Apartment;
using NatournaServer.Models.Api.Requests.Apartment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NatournaServer.Controllers
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

        /// <summary>
        /// Get all apartments with building names - Any authenticated user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ApartmentResponse>>> GetAllApartments()
        {
            List<ApartmentResponse> apartments = await _apartmentManager.GetAllApartmentsAsync();
            return Ok(apartments);
        }

        /// <summary>
        /// Get apartment by ID with building name - Any authenticated user
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApartmentResponse>> GetApartmentById(int id)
        {
            ApartmentResponse? apartment = await _apartmentManager.GetApartmentByIdAsync(id);

            if (apartment == null)
            {
                return NotFound();
            }

            return Ok(apartment);
        }

        /// <summary>
        /// Get apartments by building ID with building names - Any authenticated user
        /// </summary>
        [HttpGet("building/{buildingId}")]
        public async Task<ActionResult<List<ApartmentResponse>>> GetApartmentsByBuildingId(int buildingId)
        {
            List<ApartmentResponse> apartments = await _apartmentManager.GetApartmentsByBuildingIdAsync(buildingId);
            return Ok(apartments);
        }

        /// <summary>
        /// Create apartment - Admin only
        /// </summary>
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<ApartmentResponse>> CreateApartment(ApartmentRequest apartment)
        {
            ApartmentResponse createdApartment = await _apartmentManager.CreateApartmentAsync(apartment);
            return CreatedAtAction(nameof(GetApartmentById), new { id = createdApartment.Id }, createdApartment);
        }

        /// <summary>
        /// Update apartment - Admin only
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<ApartmentResponse>> UpdateApartment(int id, ApartmentRequest apartment)
        {
            ApartmentResponse? updatedApartment = await _apartmentManager.UpdateApartmentAsync(id, apartment);

            if (updatedApartment == null)
            {
                return NotFound();
            }

            return Ok(updatedApartment);
        }

        /// <summary>
        /// Delete apartment - Admin only
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult> DeleteApartment(int id)
        {
            bool result = await _apartmentManager.DeleteApartmentAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Set apartment active status - Admin only
        /// </summary>
        [HttpPatch("{id}/active")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<ApartmentResponse>> SetApartmentActive(int id, [FromBody] bool isActive)
        {
            ApartmentResponse? apartment = await _apartmentManager.SetApartmentActiveAsync(id, isActive);

            if (apartment == null)
            {
                return NotFound();
            }

            return Ok(apartment);
        }
    }
}