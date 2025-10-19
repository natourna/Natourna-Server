using BuildingManagement.Models.Api.Requests.Payment;
using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Api
{
    public interface IPaymentApiManager
    {
        Task<List<PaymentEntity>> GetAllPaymentsAsync();

        Task<PaymentEntity?> GetPaymentByIdAsync(int id);

        Task<List<PaymentEntity>> GetPaymentsByApartmentIdAsync(int apartmentId);

        Task<PaymentEntity> CreatePaymentAsync(PaymentRequest request);

        Task<PaymentEntity?> UpdatePaymentAsync(int id, PaymentEntity payment);

        Task<bool> DeletePaymentAsync(int id);

        Task<PaymentEntity> MarkPaymentAsPaidAsync(int paymentId);

        Task<PaymentEntity> MarkPaymentAsUnpaidAsync(int paymentId);
    }
}