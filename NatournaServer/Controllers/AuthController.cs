using NatournaServer.Constants.Error;
using NatournaServer.Interfaces.Api;
using NatournaServer.Models.Api.Requests.Login;
using NatournaServer.Models.Api.Response.Error;
using NatournaServer.Models.Api.Response.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NatournaServer.Controllers
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
                return Unauthorized(new ErrorResponse { ErrorCode = ErrorCodes.AUTH_INVALID_CREDENTIALS_ERROR, Message = "Invalid username or password" });
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
                return Unauthorized(new ErrorResponse { ErrorCode = ErrorCodes.AUTH_INVALID_TOKEN_ERROR, Message = "Invalid token" });
            }

            var response = await _authManager.RefreshTokenAsync(username);

            if (response == null)
            {
                return Unauthorized(new ErrorResponse { ErrorCode = ErrorCodes.AUTH_REFRESH_ERROR, Message = "Unable to refresh token" });
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
