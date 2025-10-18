using BuildingManagement.Interfaces.Api;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Models.Entities;

namespace BuildingManagement.Services.Api
{
    public class BalanceApiManager : IBalanceApiManager
    {
        private readonly IBalanceContextManager _balanceContextManager;

        public BalanceApiManager(IBalanceContextManager balanceContextManager)
        {
            _balanceContextManager = balanceContextManager;
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

        public async Task<BalanceEntity> CreateBalanceAsync(BalanceEntity balance)
        {
            return await _balanceContextManager.CreateAsync(balance);
        }

        public async Task<BalanceEntity?> UpdateBalanceAsync(int id, BalanceEntity balance)
        {
            return await _balanceContextManager.UpdateAsync(id, balance);
        }

        public async Task<bool> DeleteBalanceAsync(int id)
        {
            return await _balanceContextManager.DeleteAsync(id);
        }
    }
}
