using NatournaServer.Constants.Error;
using NatournaServer.Data;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Context;
using NatournaServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace NatournaServer.Services.Context
{
    public class CycleContextManager : ICycleContextManager
    {
        private readonly NatournaServerContext _context;
        private readonly ILogger<CycleContextManager> _logger;

        public CycleContextManager(NatournaServerContext context, ILogger<CycleContextManager> logger)
            => (_context, _logger) = (context, logger);

        public async Task<(List<CycleEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
        {
            try
            {
                var query = _context.Cycles.AsQueryable();

                int totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(c => c.StartDate)
                    .ThenByDescending(c => c.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to retrieve cycles", ErrorCodes.CYCLE_GET_ALL_ERROR);
                throw new ContextException(ErrorCodes.CYCLE_GET_ALL_ERROR,
                    "Failed to retrieve cycles",
                    $"Page: {page}, PageSize: {pageSize}",
                    ex);
            }
        }

        public async Task<CycleEntity?> GetActiveAsync()
        {
            try
            {
                return await _context.Cycles
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.StartDate)
                    .ThenByDescending(c => c.Id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to retrieve active cycle", ErrorCodes.CYCLE_GET_ALL_ERROR);
                throw new ContextException(ErrorCodes.CYCLE_GET_ALL_ERROR,
                    "Failed to retrieve the active cycle",
                    "Filter - IsActive: true",
                    ex);
            }
        }

        public async Task<CycleEntity?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Cycles
                    .FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to retrieve cycle with ID {CycleId}",
                    ErrorCodes.CYCLE_GET_BY_ID_ERROR, id);
                throw new ContextException(ErrorCodes.CYCLE_GET_BY_ID_ERROR,
                    $"Failed to retrieve cycle with ID {id}",
                    $"CycleId: {id}",
                    ex);
            }
        }

        public async Task<CycleEntity> CreateAsync(CycleEntity cycle)
        {
            try
            {
                _context.Cycles.Add(cycle);
                await _context.SaveChangesAsync();

                return cycle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to create cycle", ErrorCodes.CYCLE_CREATE_ERROR);
                throw new ContextException(ErrorCodes.CYCLE_CREATE_ERROR,
                    "Failed to create cycle",
                    $"Label: {cycle.Label}, Cycle: {cycle.Cycle}, Amount: {cycle.Amount}",
                    ex);
            }
        }

        public async Task<CycleEntity?> UpdateAsync(int id, string label, string? description, bool isActive)
        {
            try
            {
                var existingCycle = await _context.Cycles.FindAsync(id);
                if (existingCycle == null)
                {
                    return null;
                }

                existingCycle.Label = label;
                existingCycle.Description = description;
                existingCycle.IsActive = isActive;
                existingCycle.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return existingCycle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to update cycle with ID {CycleId}",
                    ErrorCodes.CYCLE_UPDATE_ERROR, id);
                throw new ContextException(ErrorCodes.CYCLE_UPDATE_ERROR,
                    $"Failed to update cycle with ID {id}",
                    $"CycleId: {id}",
                    ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var cycle = await _context.Cycles.FindAsync(id);
                if (cycle == null)
                {
                    return false;
                }

                _context.Cycles.Remove(cycle);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to delete cycle with ID {CycleId}",
                    ErrorCodes.CYCLE_DELETE_ERROR, id);
                throw new ContextException(ErrorCodes.CYCLE_DELETE_ERROR,
                    $"Failed to delete cycle with ID {id}",
                    $"CycleId: {id}",
                    ex);
            }
        }
    }
}
