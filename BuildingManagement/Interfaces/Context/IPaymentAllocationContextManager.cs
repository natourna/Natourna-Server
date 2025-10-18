using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Context
{
    public interface IPaymentAllocationContextManager
    {
        Task<List<PaymentAllocationEntity>> GetAllAsync(int? allocationId = null, int? paymentId = null, int? balanceId = null);
        Task<PaymentAllocationEntity> CreateAsync(PaymentAllocationEntity allocation);
        Task CreateRangeAsync(List<PaymentAllocationEntity> allocations);
        Task<PaymentAllocationEntity?> UpdateAsync(int id, PaymentAllocationEntity allocation);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByPaymentIdAsync(int paymentId);
    }
}
