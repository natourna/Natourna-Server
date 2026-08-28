using NatournaServer.Constants.User;
using NatournaServer.Interfaces.Api;
using NatournaServer.Models.Api.Response.User;
using NatournaServer.Models.Api.Requests.User;
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
        public async Task<ActionResult<List<UserResponse>>> GetAllUsers()
        {
            var users = await _userManager.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetUserById(int id)
        {
            var user = await _userManager.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var currentUserEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            var isAdmin = User.IsInRole(RoleNames.Admin);

            if (!isAdmin && user.Email != currentUserEmail)
            {
                return Forbid();
            }

            return Ok(user);
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserResponse>> GetCurrentUser()
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var user = await _userManager.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<UserResponse>> CreateUser(CreateUserRequest user)
        {
            var createdUser = await _userManager.CreateUserAsync(user);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponse>> UpdateUser(int id, UpdateUserRequest user)
        {
            var existingUser = await _userManager.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            var currentUserEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            var isAdmin = User.IsInRole(RoleNames.Admin);

            if (!isAdmin && existingUser.Email != currentUserEmail)
            {
                return Forbid();
            }

            if (!isAdmin)
            {
                user.RoleId = existingUser.RoleId;
                user.IsActive = existingUser.IsActive;
            }

            var updatedUser = await _userManager.UpdateUserAsync(id, user);
            return Ok(updatedUser);
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
    }
}
