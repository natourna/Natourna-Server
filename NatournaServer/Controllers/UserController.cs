using NatournaServer.Constants.User;
using NatournaServer.Interfaces.Api;
using NatournaServer.Models.Api.Requests.Paging;
using NatournaServer.Models.Api.Requests.User;
using NatournaServer.Models.Api.Response.Paging;
using NatournaServer.Models.Api.Response.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace NatournaServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserApiManager _userManager;

        public UserController(IUserApiManager userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<PagedResponse<UserResponse>>> GetUsers([FromQuery] PagedQuery paging, [FromQuery] string? search)
        {
            var users = await _userManager.GetUsersAsync(paging.Page, paging.PageSize, search);
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetUserById(int id)
        {
            if (!IsAdmin() && CurrentUserId() != id)
            {
                return Forbid();
            }

            var user = await _userManager.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserResponse>> GetCurrentUser()
        {
            int? userId = CurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _userManager.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
        {
            var createdUser = await _userManager.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponse>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            bool isAdmin = IsAdmin();

            if (!isAdmin && CurrentUserId() != id)
            {
                return Forbid();
            }

            if (!isAdmin)
            {
                var existing = await _userManager.GetUserByIdAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

                request.Role = existing.Role;
                request.IsActive = existing.IsActive;
            }

            var updatedUser = await _userManager.UpdateUserAsync(id, request);
            if (updatedUser == null)
            {
                return NotFound();
            }

            return Ok(updatedUser);
        }

        [HttpPatch("{id}/active")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<UserResponse>> SetUserActive(int id, [FromBody] bool isActive)
        {
            var user = await _userManager.SetUserActiveAsync(id, isActive);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var result = await _userManager.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        private bool IsAdmin()
        {
            return User.IsInRole(RoleNames.Admin);
        }

        private int? CurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out int id) ? id : null;
        }
    }
}
