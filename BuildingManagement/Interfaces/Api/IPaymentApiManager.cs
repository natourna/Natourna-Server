using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Api
{
    public interface IPaymentApiManager
    {
        Task<List<PaymentEntity>> GetAllPaymentsAsync();

        Task<PaymentEntity?> GetPaymentByIdAsync(int id);

        Task<List<PaymentEntity>> GetPaymentsByBillIdAsync(int billId);

        Task<List<PaymentEntity>> GetPaymentsByApartmentIdAsync(int apartmentId);

        Task<PaymentEntity> CreatePaymentAsync(PaymentEntity payment);

        Task<PaymentEntity?> UpdatePaymentAsync(int id, PaymentEntity payment);

        Task<bool> DeletePaymentAsync(int id);
    }
}