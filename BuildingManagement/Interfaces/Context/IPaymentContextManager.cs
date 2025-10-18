using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Context
{
    public interface IPaymentContextManager
    {
        Task<List<PaymentEntity>> GetAllAsync(int? paymentId = null, int? apartmentId = null, int? cycleId = null, bool? isPaid = null);

        Task<PaymentEntity> CreateAsync(PaymentEntity payment);

        Task<PaymentEntity?> UpdateAsync(int id, PaymentEntity payment);

        Task<bool> DeleteAsync(int id);
    }
}