using NatournaServer.Interfaces.Api;
using NatournaServer.Models.Api.Requests.Login;
using NatournaServer.Models.Api.Response.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace NatournaServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthApiManager _authManager;

        public AuthController(IAuthApiManager authManager)
        {
            _authManager = authManager;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var response = await _authManager.LoginAsync(request);

            if (response == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            return Ok(response);
        }

        [HttpPost("refresh")]
        [Authorize]
        public async Task<ActionResult<LoginResponse>> RefreshToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var response = await _authManager.RefreshTokenAsync(userId);

            if (response == null)
            {
                return Unauthorized(new { message = "Unable to refresh token" });
            }

            return Ok(response);
        }
    }
}
