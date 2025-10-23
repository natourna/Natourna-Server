using BuildingManagement.Constants.Log;
using BuildingManagement.Interfaces.Api;
using BuildingManagement.Interfaces.Authentication;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Interfaces.Services;
using BuildingManagement.Models.Api.Requests.Login;
using BuildingManagement.Models.Api.Response.Login;
using BuildingManagement.Models.Configurations;
using Microsoft.Extensions.Options;

namespace BuildingManagement.Services.Api
{
    public class AuthApiManager : IAuthApiManager
    {
        private readonly IUserContextManager _userContextManager;
        private readonly IJwtAuthenticationService _jwtService;
        private readonly IAuditService _auditService;
        private readonly JwtConfiguration _jwtSettings;
        private readonly ILogger<AuthApiManager> _logger;

        public AuthApiManager(IUserContextManager userContextManager, IJwtAuthenticationService jwtService, IAuditService auditService, IOptions<JwtConfiguration> jwtSettings, ILogger<AuthApiManager> logger)
        {
            _userContextManager = userContextManager;
            _jwtService = jwtService;
            _auditService = auditService;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            // Get user from database by email
            var user = await _userContextManager.GetByEmailAsync(request.Username);

            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Email}", request.Username);
                return null;
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt for inactive user: {Email}", request.Username);
                return null;
            }

            // Validate password (in production, use BCrypt or similar)
            if (user.Password != request.Password)
            {
                _logger.LogWarning("Failed login attempt for user: {Email}", request.Username);
                return null;
            }

            // Log successful login
            await _auditService.LogAsync(LogAction.Login, "User", user.Id);

            // Generate JWT token with role and userId
            var token = _jwtService.GenerateToken(user.Email, user.Id.ToString(), user.Role.ToString());
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

            _logger.LogInformation("User {Email} with role {Role} logged in successfully", user.Email, user.Role);

            return new LoginResponse
            {
                Token = token,
                Username = user.Email,
                ExpiresAt = expiresAt
            };
        }

        public async Task<LoginResponse?> RefreshTokenAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("Refresh token attempted with empty username");
                return null;
            }

            // Verify user still exists and is active
            var user = await _userContextManager.GetByEmailAsync(username);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Refresh token attempted for invalid/inactive user: {Email}", username);
                return null;
            }

            // Generate new token with current role and userId
            var token = _jwtService.GenerateToken(user.Email, user.Id.ToString(), user.Role.ToString());
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

            _logger.LogInformation("Token refreshed for user: {Email}", username);

            return new LoginResponse
            {
                Token = token,
                Username = user.Email,
                ExpiresAt = expiresAt
            };
        }

        public bool ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            var principal = _jwtService.ValidateToken(token);
            return principal != null;
        }
    }
}
