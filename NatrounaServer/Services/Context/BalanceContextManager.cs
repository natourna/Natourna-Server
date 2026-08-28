using NatrounaServer.Constants.Error;
using NatrounaServer.Data;
using NatrounaServer.Exceptions;
using NatrounaServer.Interfaces.Context;
using NatrounaServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace NatrounaServer.Services.Context
{
    public class BalanceContextManager : IBalanceContextManager
    {
        private readonly NatrounaServerContext _context;
        private readonly ILogger<BalanceContextManager> _logger;

        public BalanceContextManager(NatrounaServerContext context, ILogger<BalanceContextManager> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<BalanceEntity>> GetAllAsync(int? balanceId = null, int? compoundId = null)
        {
            try
            {
                _logger.LogInformation("Getting all balances with filters - BalanceId: {BalanceId}, CompoundId: {CompoundId}", balanceId, compoundId);

                var query = _context.Balances
                    .Include(b => b.PaymentAllocations)
                    .Include(b => b.Bills)
                    .AsQueryable();

                // Apply filters
                if (balanceId.HasValue)
                {
                    query = query.Where(b => b.Id == balanceId.Value);
                }

                if (compoundId.HasValue)
                {
                    query = query.Where(b => b.CompoundId == compoundId.Value);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Balance.GetAllFailed(balanceId, compoundId);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}. {TechnicalDetails}", ErrorCodes.BALANCE_GET_ALL_ERROR, userMessage, technicalDetails);

                throw new ContextException(ErrorCodes.BALANCE_GET_ALL_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<BalanceEntity?> GetByIdAsync(int id)
        {
            try
            {
                return (await GetAllAsync(balanceId: id)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Balance.GetByIdFailed(id);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_GET_BY_ID_ERROR, userMessage);

                throw new ContextException(ErrorCodes.BALANCE_GET_BY_ID_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<BalanceEntity> CreateAsync(BalanceEntity balance)
        {
            try
            {
                _logger.LogInformation("Creating new balance - Label: {Label}, CurrentAmount: {CurrentAmount}, CompoundId: {CompoundId}", balance.Label, balance.CurrentAmount, balance.CompoundId);

                _context.Balances.Add(balance);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created balance with ID {BalanceId}", balance.Id);

                return balance;
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Balance.CreateFailed(balance);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_CREATE_ERROR, userMessage);

                throw new ContextException(ErrorCodes.BALANCE_CREATE_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<BalanceEntity?> UpdateAsync(int id, BalanceEntity balance)
        {
            try
            {
                _logger.LogInformation("Updating balance with ID: {BalanceId}", id);

                var existingBalance = await _context.Balances.FindAsync(id);
                if (existingBalance == null)
                {
                    _logger.LogWarning("Cannot update - Balance with ID {BalanceId} not found", id);
                    return null;
                }

                existingBalance.Label = balance.Label;
                existingBalance.CurrentAmount = balance.CurrentAmount;
                existingBalance.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated balance with ID {BalanceId}", id);

                return existingBalance;
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Balance.UpdateFailed(id, balance);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_UPDATE_ERROR, userMessage);

                throw new ContextException(ErrorCodes.BALANCE_UPDATE_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting balance with ID: {BalanceId}", id);

                var balance = await _context.Balances.FindAsync(id);
                if (balance == null)
                {
                    _logger.LogWarning("Cannot delete - Balance with ID {BalanceId} not found", id);
                    return false;
                }

                _context.Balances.Remove(balance);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted balance with ID {BalanceId}", id);

                return true;
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Balance.DeleteFailed(id);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_DELETE_ERROR, userMessage);

                throw new ContextException(ErrorCodes.BALANCE_DELETE_ERROR, userMessage, technicalDetails, ex);
            }
        }
    }
}