using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Context
{
    public interface IPaymentContextManager
    {
        Task<List<PaymentEntity>> GetAllAsync();
        Task<PaymentEntity?> GetByIdAsync(int id);
        Task<List<PaymentEntity>> GetByBillIdAsync(int billId);
        Task<List<PaymentEntity>> GetByApartmentIdAsync(int apartmentId);
        Task<PaymentEntity> CreateAsync(PaymentEntity payment);
        Task<PaymentEntity?> UpdateAsync(int id, PaymentEntity payment);
        Task<bool> DeleteAsync(int id);
    }
}