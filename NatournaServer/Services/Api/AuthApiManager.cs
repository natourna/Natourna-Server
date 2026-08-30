using NatournaServer.Constants.Log;
using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Authentication;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Models.Api.Requests.Login;
using NatournaServer.Models.Api.Response.Login;
using NatournaServer.Models.Configurations;
using Microsoft.Extensions.Options;

namespace NatournaServer.Services.Api
{
    public class AuthApiManager : IAuthApiManager
    {
        private readonly IUserContextManager _userContextManager;
        private readonly IOrganizationContextManager _organizationContextManager;
        private readonly IJwtAuthenticationService _jwtService;
        private readonly IPasswordHashingService _passwordHashingService;
        private readonly IAuditService _auditService;
        private readonly JwtConfiguration _jwtSettings;
        private readonly ILogger<AuthApiManager> _logger;

        public AuthApiManager(IUserContextManager userContextManager, IOrganizationContextManager organizationContextManager, IJwtAuthenticationService jwtService, IPasswordHashingService passwordHashingService, IAuditService auditService, IOptions<JwtConfiguration> jwtSettings, ILogger<AuthApiManager> logger)
        {
            _userContextManager = userContextManager;
            _organizationContextManager = organizationContextManager;
            _jwtService = jwtService;
            _passwordHashingService = passwordHashingService;
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

            // Validate password against the stored hash
            if (!_passwordHashingService.VerifyPassword(user.Password, request.Password))
            {
                _logger.LogWarning("Failed login attempt for user: {Email}", request.Username);
                return null;
            }

            // Log successful login
            await _auditService.LogAsync(LogAction.Login, "User", user.Id);

            // Generate JWT token with role, userId and organization (tenant scope)
            var token = _jwtService.GenerateToken(user.Email, user.Id.ToString(), user.Role!.Name, user.OrganizationId);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

            var organization = await _organizationContextManager.GetByIdAsync(user.OrganizationId);

            _logger.LogInformation("User {Email} with role {Role} logged in successfully", user.Email, user.Role.Name);

            return new LoginResponse
            {
                Token = token,
                Username = user.Email,
                ExpiresAt = expiresAt,
                OrganizationName = organization?.Name ?? string.Empty
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

            // Generate new token with current role, userId and organization
            var token = _jwtService.GenerateToken(user.Email, user.Id.ToString(), user.Role!.Name, user.OrganizationId);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

            var organization = await _organizationContextManager.GetByIdAsync(user.OrganizationId);

            _logger.LogInformation("Token refreshed for user: {Email}", username);

            return new LoginResponse
            {
                Token = token,
                Username = user.Email,
                ExpiresAt = expiresAt,
                OrganizationName = organization?.Name ?? string.Empty
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
