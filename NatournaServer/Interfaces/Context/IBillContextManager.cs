using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Context
{
    public interface IBillContextManager
    {
        Task<(List<BillEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, bool? isPaid = null);

        Task<BillEntity?> GetByIdAsync(int id);

        Task<BillEntity> CreateAsync(BillEntity bill);

        Task<BillEntity?> UpdateAsync(int id, string label, decimal amount, DateTime? dueDate);

        Task<BillEntity?> SetPaidStatusAsync(int id, bool isPaid, DateTime? paymentDate);

        Task<bool> DeleteAsync(int id);
    }
}
