using NatournaServer.Constants.User;
using NatournaServer.Interfaces.Api;
using NatournaServer.Models.Api.Response.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NatournaServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly IRoleApiManager _roleManager;

        public RoleController(IRoleApiManager roleManager)
        {
            _roleManager = roleManager;
        }

        /// <summary>
        /// Get all roles - Admin only
        /// </summary>
        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<List<RoleResponse>>> GetAllRoles()
        {
            var roles = await _roleManager.GetAllRolesAsync();
            return Ok(roles);
        }
    }
}
