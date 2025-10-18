using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Context
{
    public interface IBalanceContextManager
    {
        Task<List<BalanceEntity>> GetAllAsync(int? balanceId = null, int? compoundId = null);

        Task<BalanceEntity> CreateAsync(BalanceEntity balance);

        Task<BalanceEntity?> UpdateAsync(int id, BalanceEntity balance);

        Task<bool> DeleteAsync(int id);
    }
}