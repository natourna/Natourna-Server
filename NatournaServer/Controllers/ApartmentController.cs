using NatournaServer.Constants.User;
using NatournaServer.Interfaces.Api;
using NatournaServer.Models.Api.Requests.Apartment;
using NatournaServer.Models.Api.Requests.Paging;
using NatournaServer.Models.Api.Response.Apartment;
using NatournaServer.Models.Api.Response.Paging;
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

        [HttpGet]
        public async Task<ActionResult<PagedResponse<ApartmentResponse>>> GetApartments([FromQuery] PagedQuery paging, [FromQuery] int? buildingId, [FromQuery] string? search)
        {
            var apartments = await _apartmentManager.GetApartmentsAsync(paging.Page, paging.PageSize, buildingId, search);
            return Ok(apartments);
        }

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

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<ApartmentResponse>> CreateApartment([FromBody] ApartmentRequest request)
        {
            ApartmentResponse createdApartment = await _apartmentManager.CreateApartmentAsync(request);
            return CreatedAtAction(nameof(GetApartmentById), new { id = createdApartment.Id }, createdApartment);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<ApartmentResponse>> UpdateApartment(int id, [FromBody] ApartmentRequest request)
        {
            ApartmentResponse? updatedApartment = await _apartmentManager.UpdateApartmentAsync(id, request);

            if (updatedApartment == null)
            {
                return NotFound();
            }

            return Ok(updatedApartment);
        }

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
