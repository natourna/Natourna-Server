using NatournaServer.Constants.Error;
using NatournaServer.Constants.Log;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Models.Api.Requests.Balance;
using NatournaServer.Models.Api.Response.Balance;
using NatournaServer.Models.Entities;

namespace NatournaServer.Services.Api
{
    public class BalanceApiManager : IBalanceApiManager
    {
        private readonly IBalanceContextManager _balanceContextManager;
        private readonly ICompoundContextManager _compoundContextManager;
        private readonly IAuditService _auditService;

        public BalanceApiManager(IBalanceContextManager balanceContextManager, ICompoundContextManager compoundContextManager, IAuditService auditService)
        {
            _balanceContextManager = balanceContextManager;
            _compoundContextManager = compoundContextManager;
            _auditService = auditService;
        }

        public async Task<List<BalanceResponse>> GetAllBalancesAsync(int? compoundId)
        {
            var balances = await _balanceContextManager.GetAllAsync(compoundId: compoundId);
            return balances.Select(MapToResponse).ToList();
        }

        public async Task<BalanceResponse?> GetBalanceByIdAsync(int id)
        {
            var balance = await _balanceContextManager.GetByIdAsync(id);
            return balance == null ? null : MapToResponse(balance);
        }

        public async Task<BalanceResponse> CreateBalanceAsync(BalanceRequest request)
        {
            await EnsureCompoundExistsAsync(request.CompoundId);

            var balance = new BalanceEntity(request.Label, request.CompoundId)
            {
                CurrentAmount = request.CurrentAmount
            };

            var created = await _balanceContextManager.CreateAsync(balance);

            await _auditService.LogAsync(LogAction.Create, "Balance", created.Id, null, new { created.CompoundId, created.CurrentAmount, created.Label });

            return MapToResponse(created);
        }

        public async Task<BalanceResponse?> UpdateBalanceAsync(int id, BalanceRequest request)
        {
            var existing = await _balanceContextManager.GetByIdAsync(id);
            if (existing == null)
                return null;

            var oldValues = new
            {
                existing.CompoundId,
                existing.CurrentAmount,
                existing.Label
            };

            var balance = new BalanceEntity(request.Label, request.CompoundId)
            {
                CurrentAmount = request.CurrentAmount
            };

            var updated = await _balanceContextManager.UpdateAsync(id, balance);

            if (updated == null)
                return null;

            await _auditService.LogAsync(LogAction.Update, "Balance", id, oldValues, new { updated.CompoundId, updated.CurrentAmount, updated.Label });

            return MapToResponse(updated);
        }

        public async Task<bool> DeleteBalanceAsync(int id)
        {
            var existing = await _balanceContextManager.GetByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            await _auditService.LogAsync(LogAction.Delete, "Balance", id, new { existing.CompoundId, existing.CurrentAmount }, null);

            return await _balanceContextManager.DeleteAsync(id);
        }

        private async Task EnsureCompoundExistsAsync(int compoundId)
        {
            var compound = await _compoundContextManager.GetByIdAsync(compoundId);
            if (compound == null)
            {
                throw new ApiException(ErrorCodes.BALANCE_COMPOUND_INVALID_ERROR, "The requested compound does not exist", $"CompoundId: {compoundId}");
            }
        }

        private static BalanceResponse MapToResponse(BalanceEntity balance)
        {
            return new BalanceResponse
            {
                Id = balance.Id,
                Label = balance.Label,
                CurrentAmount = balance.CurrentAmount,
                CompoundId = balance.CompoundId,
                CreatedAt = balance.CreatedAt,
                UpdatedAt = balance.UpdatedAt
            };
        }
    }
}
