using NatournaServer.Constants.Log;

namespace NatournaServer.Interfaces.Services
{
    public interface IAuditService
    {
        Task LogAsync(LogAction action, string entityType, int? entityId = null, object? oldValues = null, object? newValues = null);
    }
}
