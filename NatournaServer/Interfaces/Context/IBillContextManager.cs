using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Context
{
    public interface IBillContextManager
    {
        Task<List<BillEntity>> GetAllAsync(int? billId = null, int? balanceId = null, bool? isPaid = null, DateTime? dueDateFrom = null, DateTime? dueDateTo = null);

        Task<(List<BillEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, int? balanceId = null, bool? isPaid = null);

        Task<BillEntity?> GetByIdAsync(int id);

        Task<List<BillEntity>> GetByBalanceIdAsync(int balanceId);

        Task<bool> AnyAsync(int? balanceId = null, int? compoundId = null);

        Task<BillEntity> CreateAsync(BillEntity bill);

        Task<BillEntity?> UpdateAsync(int id, BillEntity bill);

        Task<bool> DeleteAsync(int id);
    }
}