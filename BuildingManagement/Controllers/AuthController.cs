using BuildingManagement.Interfaces.Api;
using BuildingManagement.Models.Api.Requests.Login;
using BuildingManagement.Models.Api.Response.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildingManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthApiManager _authManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthApiManager authManager,
            ILogger<AuthController> logger)
        {
            _authManager = authManager;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
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
            var username = User.Identity?.Name;

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var response = await _authManager.RefreshTokenAsync(username);

            if (response == null)
            {
                return Unauthorized(new { message = "Unable to refresh token" });
            }

            return Ok(response);
        }

        [HttpGet("validate")]
        [Authorize]
        public ActionResult ValidateToken()
        {
            return Ok(new { message = "Token is valid", username = User.Identity?.Name });
        }
    }
}
