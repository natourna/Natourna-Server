using NatrounaServer.Models.Api.Requests.Payment;
using NatrounaServer.Models.Api.Response.Payment;
using NatrounaServer.Models.Entities;

namespace NatrounaServer.Interfaces.Api
{
    public interface IPaymentApiManager
    {
        Task<List<PaymentResponse>> GetAllPaymentsAsync();

        Task<PaymentResponse?> GetPaymentByIdAsync(int id);

        Task<List<PaymentResponse>> GetPaymentsByApartmentIdAsync(int apartmentId);

        Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request);

        Task<PaymentResponse?> UpdatePaymentAsync(int id, PaymentEntity payment);

        Task<bool> DeletePaymentAsync(int id);

        Task<PaymentResponse> MarkPaymentAsPaidAsync(int paymentId);

        Task<PaymentResponse> MarkPaymentAsUnpaidAsync(int paymentId);
    }
}