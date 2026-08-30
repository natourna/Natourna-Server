using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Context
{
    public interface IPaymentContextManager
    {
        Task<List<PaymentEntity>> GetAllAsync(int? paymentId = null, int? apartmentId = null, int? cycleId = null, bool? isPaid = null);

        Task<(List<PaymentEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, int? apartmentId = null, int? cycleId = null, bool? isPaid = null);

        Task<PaymentEntity?> GetByIdAsync(int id);

        Task<PaymentEntity> CreateAsync(PaymentEntity payment);

        Task<List<PaymentEntity>> CreateRangeAsync(List<PaymentEntity> payments);

        Task<bool> AnyAsync(int? apartmentId = null, int? cycleId = null, int? buildingId = null, int? compoundId = null);

        Task<PaymentEntity?> UpdateAsync(int id, PaymentEntity payment);

        Task<bool> DeleteAsync(int id);
    }
}