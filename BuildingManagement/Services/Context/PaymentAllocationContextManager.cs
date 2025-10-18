using BuildingManagement.Constants;
using BuildingManagement.Data;
using BuildingManagement.Exceptions;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuildingManagement.Services.Context
{
    public class PaymentAllocationContextManager : IPaymentAllocationContextManager
    {
        private readonly BuildingManagementContext _context;
        private readonly ILogger<PaymentAllocationContextManager> _logger;

        public PaymentAllocationContextManager(BuildingManagementContext context, ILogger<PaymentAllocationContextManager> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PaymentAllocationEntity>> GetAllAsync(
            int? allocationId = null,
            int? paymentId = null,
            int? balanceId = null)
        {
            try
            {
                _logger.LogInformation("Getting all payment allocations with filters - AllocationId: {AllocationId}, PaymentId: {PaymentId}, BalanceId: {BalanceId}",
                    allocationId, paymentId, balanceId);

                var query = _context.PaymentAllocations
                    .Include(pa => pa.Payment)
                    .Include(pa => pa.Balance)
                    .AsQueryable();

                // Apply filters
                if (allocationId.HasValue)
                {
                    query = query.Where(pa => pa.Id == allocationId.Value);
                }

                if (paymentId.HasValue)
                {
                    query = query.Where(pa => pa.PaymentId == paymentId.Value);
                }

                if (balanceId.HasValue)
                {
                    query = query.Where(pa => pa.BalanceId == balanceId.Value);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to retrieve payment allocations", ErrorCodes.PAYMENT_GET_ALL_ERROR);
                throw new ContextException(ErrorCodes.PAYMENT_GET_ALL_ERROR,
                    "Failed to retrieve payment allocations",
                    $"Filters - AllocationId: {allocationId}, PaymentId: {paymentId}, BalanceId: {balanceId}",
                    ex);
            }
        }

        public async Task<PaymentAllocationEntity> CreateAsync(PaymentAllocationEntity allocation)
        {
            try
            {
                _logger.LogInformation("Creating new payment allocation - PaymentId: {PaymentId}, BalanceId: {BalanceId}, Percentage: {Percentage}%",
                    allocation.PaymentId, allocation.BalanceId, allocation.Percentage);

                _context.PaymentAllocations.Add(allocation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created payment allocation with ID {AllocationId}", allocation.Id);

                return allocation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to create payment allocation", ErrorCodes.PAYMENT_CREATE_ERROR);
                throw new ContextException(ErrorCodes.PAYMENT_CREATE_ERROR,
                    "Failed to create payment allocation",
                    $"PaymentId: {allocation.PaymentId}, BalanceId: {allocation.BalanceId}, Percentage: {allocation.Percentage}%",
                    ex);
            }
        }

        public async Task CreateRangeAsync(List<PaymentAllocationEntity> allocations)
        {
            try
            {
                _logger.LogInformation("Creating {Count} payment allocations", allocations.Count);

                _context.PaymentAllocations.AddRange(allocations);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created {Count} payment allocations", allocations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to create payment allocations", ErrorCodes.PAYMENT_CREATE_ERROR);
                throw new ContextException(ErrorCodes.PAYMENT_CREATE_ERROR,
                    "Failed to create payment allocations",
                    $"Count: {allocations.Count}",
                    ex);
            }
        }

        public async Task<PaymentAllocationEntity?> UpdateAsync(int id, PaymentAllocationEntity allocation)
        {
            try
            {
                _logger.LogInformation("Updating payment allocation with ID: {AllocationId}", id);

                var existingAllocation = await _context.PaymentAllocations.FindAsync(id);
                if (existingAllocation == null)
                {
                    _logger.LogWarning("Cannot update - Payment allocation with ID {AllocationId} not found", id);
                    return null;
                }

                existingAllocation.PaymentId = allocation.PaymentId;
                existingAllocation.BalanceId = allocation.BalanceId;
                existingAllocation.Percentage = allocation.Percentage;
                existingAllocation.AllocatedAmount = allocation.AllocatedAmount;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated payment allocation with ID {AllocationId}", id);

                return existingAllocation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to update payment allocation with ID {AllocationId}", ErrorCodes.PAYMENT_UPDATE_ERROR, id);
                throw new ContextException(ErrorCodes.PAYMENT_UPDATE_ERROR,
                    $"Failed to update payment allocation with ID {id}",
                    $"AllocationId: {id}",
                    ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting payment allocation with ID: {AllocationId}", id);

                var allocation = await _context.PaymentAllocations.FindAsync(id);
                if (allocation == null)
                {
                    _logger.LogWarning("Cannot delete - Payment allocation with ID {AllocationId} not found", id);
                    return false;
                }

                _context.PaymentAllocations.Remove(allocation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted payment allocation with ID {AllocationId}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to delete payment allocation with ID {AllocationId}", ErrorCodes.PAYMENT_DELETE_ERROR, id);
                throw new ContextException(ErrorCodes.PAYMENT_DELETE_ERROR,
                    $"Failed to delete payment allocation with ID {id}",
                    $"AllocationId: {id}",
                    ex);
            }
        }

        public async Task<bool> DeleteByPaymentIdAsync(int paymentId)
        {
            try
            {
                _logger.LogInformation("Deleting all payment allocations for payment ID: {PaymentId}", paymentId);

                var allocations = await _context.PaymentAllocations
                    .Where(pa => pa.PaymentId == paymentId)
                    .ToListAsync();

                if (!allocations.Any())
                {
                    _logger.LogWarning("No payment allocations found for payment ID {PaymentId}", paymentId);
                    return false;
                }

                _context.PaymentAllocations.RemoveRange(allocations);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted {Count} payment allocations for payment ID {PaymentId}",
                    allocations.Count, paymentId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ErrorCode}] Failed to delete payment allocations for payment ID {PaymentId}",
                    ErrorCodes.PAYMENT_DELETE_ERROR, paymentId);
                throw new ContextException(ErrorCodes.PAYMENT_DELETE_ERROR,
                    $"Failed to delete payment allocations for payment ID {paymentId}",
                    $"PaymentId: {paymentId}",
                    ex);
            }
        }
    }
}
