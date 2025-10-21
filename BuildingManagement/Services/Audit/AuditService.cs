using BuildingManagement.Constants.Log;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Interfaces.Services;
using BuildingManagement.Models.Entities;
using System.Security.Claims;
using System.Text.Json;

namespace BuildingManagement.Services.Audit
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

                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();

                var log = new AuditEntity(userId, userEmail, action, entityType, entityId)
                {
                    OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                    NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                await _logContextManager.CreateAsync(log);

                _logger.LogInformation("Audit log created: User={Email}, Action={Action}, Entity={EntityType}, EntityId={EntityId}", userEmail, action, entityType, entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create audit log for action: {Action}, entity: {EntityType}", action, entityType);
            }
        }
    }
}
