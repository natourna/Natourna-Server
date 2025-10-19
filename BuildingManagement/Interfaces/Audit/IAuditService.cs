using BuildingManagement.Constants.Log;

namespace BuildingManagement.Interfaces.Services
{
    public interface IAuditService
    {
        Task LogAsync(LogAction action, string entityType, int? entityId = null, object? oldValues = null, object? newValues = null);
    }
}
