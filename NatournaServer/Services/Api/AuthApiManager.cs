using NatournaServer.Constants.Log;
using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Authentication;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Models.Api.Requests.Login;
using NatournaServer.Models.Api.Response.Login;
using NatournaServer.Models.Api.Response.User;
using NatournaServer.Models.Configurations;
using NatournaServer.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace NatournaServer.Services.Api
{
    public class AuthApiManager : IAuthApiManager
    {
        private readonly IUserContextManager _userContextManager;
        private readonly IJwtAuthenticationService _jwtService;
        private readonly IPasswordHasher<UserEntity> _passwordHasher;
        private readonly IAuditService _auditService;
        private readonly JwtConfiguration _jwtSettings;
        private readonly ILogger<AuthApiManager> _logger;

        public AuthApiManager(IUserContextManager userContextManager, IJwtAuthenticationService jwtService, IPasswordHasher<UserEntity> passwordHasher, IAuditService auditService, IOptions<JwtConfiguration> jwtSettings, ILogger<AuthApiManager> logger)
        {
            _userContextManager = userContextManager;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
            _auditService = auditService;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userContextManager.GetByEmailAsync(request.Username);

            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Email}", request.Username);
                return null;
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt for inactive user: {Email}", request.Username);
                return null;
            }

            var verification = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);

            if (verification == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Failed login attempt for user: {Email}", request.Username);
                return null;
            }

            if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            {
                await _userContextManager.UpdatePasswordHashAsync(user.Id, _passwordHasher.HashPassword(user, request.Password));
            }

            await _auditService.LogAsync(LogAction.Login, "User", user.Id);

            _logger.LogInformation("User {Email} with role {Role} logged in successfully", user.Email, user.Role?.Name);

            return BuildLoginResponse(user);
        }

        public async Task<LoginResponse?> RefreshTokenAsync(int userId)
        {
            var user = await _userContextManager.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Refresh token attempted for invalid or inactive user: {UserId}", userId);
                return null;
            }

            _logger.LogInformation("Token refreshed for user: {Email}", user.Email);

            return BuildLoginResponse(user);
        }

        private LoginResponse BuildLoginResponse(UserEntity user)
        {
            string roleName = user.Role?.Name ?? string.Empty;
            var token = _jwtService.GenerateToken(user.Email, user.Id.ToString(), roleName);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

            return new LoginResponse
            {
                Token = token,
                Username = user.Email,
                ExpiresAt = expiresAt,
                User = new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = roleName,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                }
            };
        }
    }
}
