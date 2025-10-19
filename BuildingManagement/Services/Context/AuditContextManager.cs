using BuildingManagement.Constants.Log;
using BuildingManagement.Data;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuildingManagement.Services.Context
{
    public class AuditContextManager : ILogContextManager
    {
        private readonly BuildingManagementContext _context;

        public AuditContextManager(BuildingManagementContext context)
        {
            _context = context;
        }

        public async Task<LogEntity> CreateAsync(LogEntity log)
        {
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
            return log;
        }

        public async Task<List<LogEntity>> GetAllAsync()
        {
            return await _context.Logs
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<LogEntity>> GetByUserIdAsync(int userId)
        {
            return await _context.Logs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<LogEntity>> GetByEntityTypeAsync(string entityType)
        {
            return await _context.Logs
                .Where(l => l.EntityType == entityType)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<LogEntity>> GetByActionAsync(LogAction action)
        {
            return await _context.Logs
                .Where(l => l.Action == action)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<LogEntity>> GetRecentAsync(int count = 100)
        {
            return await _context.Logs
                .OrderByDescending(l => l.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}
