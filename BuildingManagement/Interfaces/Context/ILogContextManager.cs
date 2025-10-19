using BuildingManagement.Constants.Log;
using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Context
{
    public interface ILogContextManager
    {
        Task<LogEntity> CreateAsync(LogEntity log);

        Task<List<LogEntity>> GetAllAsync();

        Task<List<LogEntity>> GetByUserIdAsync(int userId);

        Task<List<LogEntity>> GetByEntityTypeAsync(string entityType);

        Task<List<LogEntity>> GetByActionAsync(LogAction action);

        Task<List<LogEntity>> GetRecentAsync(int count = 100);
    }
}
