using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Api
{
    public interface IBalanceApiManager
    {
        Task<List<BalanceEntity>> GetAllBalancesAsync();

        Task<BalanceEntity?> GetBalanceByIdAsync(int id);

        Task<List<BalanceEntity>> GetBalancesByCompoundIdAsync(int compoundId);

        Task<BalanceEntity> CreateBalanceAsync(BalanceEntity balance);

        Task<BalanceEntity?> UpdateBalanceAsync(int id, BalanceEntity balance);

        Task<bool> DeleteBalanceAsync(int id);
    }
}
