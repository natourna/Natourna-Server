using NatrounaServer.Constants.Log;

namespace NatrounaServer.Interfaces.Services
{
    public interface IAuditService
    {
        Task LogAsync(LogAction action, string entityType, int? entityId = null, object? oldValues = null, object? newValues = null);
    }
}
