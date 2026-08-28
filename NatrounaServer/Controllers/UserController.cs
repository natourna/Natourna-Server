using NatrounaServer.Interfaces.Api;
using NatrounaServer.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace NatrounaServer.Controllers
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
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserEntity>>> GetAllUsers()
        {
            var users = await _userManager.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserEntity>> GetUserById(int id)
        {
            var user = await _userManager.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var currentUserEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && user.Email != currentUserEmail)
            {
                return Forbid();
            }

            return Ok(user);
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserEntity>> GetCurrentUser()
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var users = await _userManager.GetAllUsersAsync();
            var user = users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserEntity>> CreateUser(UserEntity user)
        {
            var createdUser = await _userManager.CreateUserAsync(user);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserEntity>> UpdateUser(int id, UserEntity user)
        {
            var existingUser = await _userManager.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            var currentUserEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && existingUser.Email != currentUserEmail)
            {
                return Forbid();
            }

            if (!isAdmin)
            {
                user.Role = existingUser.Role;
                user.IsActive = existingUser.IsActive;
            }

            var updatedUser = await _userManager.UpdateUserAsync(id, user);
            return Ok(updatedUser);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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
