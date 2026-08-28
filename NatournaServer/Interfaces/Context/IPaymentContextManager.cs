using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Context
{
    public interface IPaymentContextManager
    {
        Task<(List<PaymentEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, int? apartmentId = null, bool? isPaid = null, DateTime? dueBefore = null);

        Task<PaymentEntity?> GetByIdAsync(int id);

        Task<PaymentEntity> CreateAsync(PaymentEntity payment);

        Task<PaymentEntity?> UpdateAsync(int id, string label, decimal amount, DateTime? dueDate, int apartmentId);

        Task<PaymentEntity?> SetPaidStatusAsync(int id, bool isPaid, DateTime? paymentDate);

        Task<bool> DeleteAsync(int id);
    }
}
