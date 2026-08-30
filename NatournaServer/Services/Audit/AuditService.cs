using NatournaServer.Authentication;
using NatournaServer.Constants.Log;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Models.Entities;
using System.Security.Claims;
using System.Text.Json;

namespace NatournaServer.Services.Audit
{
    public class AuditService : IAuditService
    {
        private readonly ILogContextManager _logContextManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditService> _logger;

        public AuditService(ILogContextManager logContextManager, IHttpContextAccessor httpContextAccessor, ILogger<AuditService> logger)
        {
            _logContextManager = logContextManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task LogAsync(LogAction action, string entityType, int? entityId = null, object? oldValues = null, object? newValues = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger.LogWarning("HttpContext is null, cannot log audit entry");
                    return;
                }

                var userEmail = httpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                int? userId = null;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedUserId))
                {
                    userId = parsedUserId;
                }

                // Best-effort tenant stamp - login audits run before authentication and stay null
                int? organizationId = null;
                var orgIdClaim = httpContext.User.FindFirst(CustomClaimTypes.OrganizationId)?.Value;
                if (!string.IsNullOrEmpty(orgIdClaim) && int.TryParse(orgIdClaim, out var parsedOrganizationId))
                {
                    organizationId = parsedOrganizationId;
                }

                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();

                var log = new AuditEntity(userEmail, action, entityType)
                {
                    UserId = userId,
                    OrganizationId = organizationId,
                    OldValues = Truncate(oldValues != null ? JsonSerializer.Serialize(oldValues) : null, 500),
                    NewValues = Truncate(newValues != null ? JsonSerializer.Serialize(newValues) : null, 500),
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    EntityId = entityId
                };

                await _logContextManager.CreateAsync(log);

                _logger.LogInformation("Audit log created: User={Email}, Action={Action}, Entity={EntityType}, EntityId={EntityId}", userEmail, action, entityType, entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create audit log for action: {Action}, entity: {EntityType}", action, entityType);
            }
        }

        // Old/NewValues columns are capped at 500 chars; a lost tail beats a lost audit row
        private static string? Truncate(string? value, int maxLength)
        {
            if (value == null || value.Length <= maxLength)
            {
                return value;
            }

            return value[..maxLength];
        }
    }
}
