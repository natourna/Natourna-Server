using NatournaServer.Constants.User;
using NatournaServer.Extensions;
using NatournaServer.Interfaces.Api;
using NatournaServer.Models.Api.Requests.Organization;
using NatournaServer.Models.Api.Response.Login;
using NatournaServer.Models.Api.Response.Organization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace NatournaServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationApiManager _organizationManager;

        public OrganizationController(IOrganizationApiManager organizationManager)
        {
            _organizationManager = organizationManager;
        }

        /// <summary>
        /// Get the caller's organization with its subscription summary - Any authenticated user
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<OrganizationResponse>> GetMyOrganization()
        {
            OrganizationResponse? organization = await _organizationManager.GetMyOrganizationAsync();

            if (organization == null)
            {
                return NotFound();
            }

            return Ok(organization);
        }

        /// <summary>
        /// Update organization name and LBP exchange rate - Admin only
        /// </summary>
        [HttpPut("settings")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<OrganizationResponse>> UpdateSettings(UpdateOrganizationSettingsRequest request)
        {
            OrganizationResponse? organization = await _organizationManager.UpdateSettingsAsync(request);

            if (organization == null)
            {
                return NotFound();
            }

            return Ok(organization);
        }

        /// <summary>
        /// Self-service signup: organization + trial subscription + admin + compound + buildings.
        /// Gated by Registration:Enabled (404 when off). Returns an auto-login token.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting(SecurityExtension.AuthRateLimitPolicy)]
        public async Task<ActionResult<LoginResponse>> Register(RegisterOrganizationRequest request)
        {
            LoginResponse response = await _organizationManager.RegisterAsync(request);
            return Ok(response);
        }
    }
}
