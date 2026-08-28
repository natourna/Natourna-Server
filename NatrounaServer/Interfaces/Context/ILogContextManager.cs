using NatrounaServer.Constants.Log;
using NatrounaServer.Models.Entities;

namespace NatrounaServer.Interfaces.Context
{
    public interface ILogContextManager
    {
        Task<AuditEntity> CreateAsync(AuditEntity log);

        Task<List<AuditEntity>> GetAllAsync();

        Task<List<AuditEntity>> GetByUserIdAsync(int userId);

        Task<List<AuditEntity>> GetByEntityTypeAsync(string entityType);

        Task<List<AuditEntity>> GetByActionAsync(LogAction action);

        Task<List<AuditEntity>> GetRecentAsync(int count = 100);
    }
}
