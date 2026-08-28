using NatrounaServer.Models.Entities;

namespace NatrounaServer.Interfaces.Context
{
    public interface IBalanceContextManager
    {
        Task<List<BalanceEntity>> GetAllAsync(int? balanceId = null, int? compoundId = null);

        Task<BalanceEntity?> GetByIdAsync(int id);

        Task<BalanceEntity> CreateAsync(BalanceEntity balance);

        Task<BalanceEntity?> UpdateAsync(int id, BalanceEntity balance);

        Task<bool> DeleteAsync(int id);
    }
}