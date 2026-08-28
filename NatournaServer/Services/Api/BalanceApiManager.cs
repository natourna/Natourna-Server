using NatournaServer.Constants.Log;
using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Models.Api.Requests.Balance;
using NatournaServer.Models.Entities;

namespace NatournaServer.Services.Api
{
    public class BalanceApiManager : IBalanceApiManager
    {
        private readonly IBalanceContextManager _balanceContextManager;
        private readonly IAuditService _auditService;

        public BalanceApiManager(IBalanceContextManager balanceContextManager, IAuditService auditService)
        {
            _balanceContextManager = balanceContextManager;
            _auditService = auditService;
        }

        public async Task<List<BalanceEntity>> GetAllBalancesAsync()
        {
            return await _balanceContextManager.GetAllAsync();
        }

        public async Task<BalanceEntity?> GetBalanceByIdAsync(int id)
        {
            var balances = await _balanceContextManager.GetAllAsync(balanceId: id);
            return balances.FirstOrDefault();
        }

        public async Task<List<BalanceEntity>> GetBalancesByCompoundIdAsync(int compoundId)
        {
            return await _balanceContextManager.GetAllAsync(compoundId: compoundId);
        }

        public async Task<BalanceEntity> CreateBalanceAsync(BalanceRequest balance)
        {
            var created = await _balanceContextManager.CreateAsync(MapToEntity(balance));

            await _auditService.LogAsync(LogAction.Create, "Balance", created.Id, null, new { created.CompoundId, created.CurrentAmount, created.Label });

            return created;
        }

        public async Task<BalanceEntity?> UpdateBalanceAsync(int id, BalanceRequest balance)
        {
            var existing = await GetBalanceByIdAsync(id);
            if (existing == null)
                return null;

            var oldValues = new
            {
                existing.CompoundId,
                existing.CurrentAmount,
                existing.Label
            };

            var updated = await _balanceContextManager.UpdateAsync(id, MapToEntity(balance));

            if (updated != null)
            {
                await _auditService.LogAsync(LogAction.Update, "Balance", id, oldValues, new { updated.CompoundId, updated.CurrentAmount, updated.Label });
            }

            return updated;
        }

        private static BalanceEntity MapToEntity(BalanceRequest request)
        {
            return new BalanceEntity(request.Label, request.CompoundId)
            {
                CurrentAmount = request.CurrentAmount
            };
        }

        public async Task<bool> DeleteBalanceAsync(int id)
        {
            var existing = await GetBalanceByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            await _auditService.LogAsync(LogAction.Delete, "Balance", id, new { existing.CompoundId, existing.CurrentAmount }, null);

            return await _balanceContextManager.DeleteAsync(id);
        }
    }
}
