using BuildingManagement.Constants.Error;
using BuildingManagement.Data;
using BuildingManagement.Exceptions;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuildingManagement.Services.Context
{
    public class CycleContextManager : ICycleContextManager
    {
        private readonly BuildingManagementContext _context;
        private readonly ILogger<CycleContextManager> _logger;

        public CycleContextManager(BuildingManagementContext context, ILogger<CycleContextManager> logger)
            => (_context, _logger) = (context, logger);

        public async Task<List<CycleEntity>> GetAllAsync(int? cycleId = null, bool? isActive = null)
        {
            try
            {
                _logger.LogInformation("Getting all cycles with filters - CycleId: {CycleId}, IsActive: {IsActive}",
                    cycleId, isActive);

                var query = _context.Cycles
                    .Include(c => c.Payments)
                    .AsQueryable();

                // Apply filters
                if (cycleId.HasValue)
                {
                    query = query.Where(c => c.Id == cycleId.Value);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(c => c.IsActive == isActive.Value);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to retrieve cycles", ErrorCodes.CYCLE_GET_ALL_ERROR);
                throw new ContextException(ErrorCodes.CYCLE_GET_ALL_ERROR,
                    "Failed to retrieve cycles",
                    $"Filters - CycleId: {cycleId}, IsActive: {isActive}",
                    ex);
            }
        }

        public async Task<CycleEntity?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting cycle by ID: {CycleId}", id);

                return await _context.Cycles
                    .Include(c => c.Payments)
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
                _logger.LogInformation("Creating new cycle - Label: {Label}, Cycle: {CycleType}, StartDate: {StartDate}, EndDate: {EndDate}",
                    cycle.Label, cycle.Cycle, cycle.StartDate, cycle.EndDate);

                _context.Cycles.Add(cycle);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created cycle with ID {CycleId}", cycle.Id);

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

        public async Task<CycleEntity?> UpdateAsync(int id, CycleEntity cycle)
        {
            try
            {
                _logger.LogInformation("Updating cycle with ID: {CycleId}", id);

                var existingCycle = await _context.Cycles.FindAsync(id);
                if (existingCycle == null)
                {
                    _logger.LogWarning("Cannot update - Cycle with ID {CycleId} not found", id);
                    return null;
                }

                existingCycle.Label = cycle.Label;
                existingCycle.Description = cycle.Description;
                existingCycle.Cycle = cycle.Cycle;
                existingCycle.StartDate = cycle.StartDate;
                existingCycle.EndDate = cycle.EndDate;
                existingCycle.ApartmentIdsCsv = cycle.ApartmentIdsCsv;
                existingCycle.Amount = cycle.Amount;
                existingCycle.IsActive = cycle.IsActive;
                existingCycle.BalanceAllocationsJson = cycle.BalanceAllocationsJson;
                existingCycle.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated cycle with ID {CycleId}", id);

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
                _logger.LogInformation("Deleting cycle with ID: {CycleId}", id);

                var cycle = await _context.Cycles.FindAsync(id);
                if (cycle == null)
                {
                    _logger.LogWarning("Cannot delete - Cycle with ID {CycleId} not found", id);
                    return false;
                }

                _context.Cycles.Remove(cycle);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted cycle with ID {CycleId}", id);

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




