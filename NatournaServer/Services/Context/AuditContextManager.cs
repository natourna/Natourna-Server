using NatournaServer.Constants.Log;
using NatournaServer.Data;
using NatournaServer.Interfaces.Context;
using NatournaServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace NatournaServer.Services.Context
{
    public class AuditContextManager : ILogContextManager
    {
        private readonly NatournaServerContext _context;

        public AuditContextManager(NatournaServerContext context)
        {
            _context = context;
        }

        public async Task<AuditEntity> CreateAsync(AuditEntity log)
        {
            _context.Audits.Add(log);
            await _context.SaveChangesAsync();
            return log;
        }

        public async Task<List<AuditEntity>> GetAllAsync()
        {
            return await _context.Audits
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AuditEntity>> GetByUserIdAsync(int userId)
        {
            return await _context.Audits
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AuditEntity>> GetByEntityTypeAsync(string entityType)
        {
            return await _context.Audits
                .Where(l => l.EntityType == entityType)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AuditEntity>> GetByActionAsync(LogAction action)
        {
            return await _context.Audits
                .Where(l => l.Action == action)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AuditEntity>> GetRecentAsync(int count = 100)
        {
            return await _context.Audits
                .OrderByDescending(l => l.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}
