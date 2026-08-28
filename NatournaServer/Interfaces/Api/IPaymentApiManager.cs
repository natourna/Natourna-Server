using NatournaServer.Models.Api.Requests.Payment;
using NatournaServer.Models.Api.Response.Paging;
using NatournaServer.Models.Api.Response.Payment;

namespace NatournaServer.Interfaces.Api
{
    public interface IPaymentApiManager
    {
        Task<PagedResponse<PaymentResponse>> GetPaymentsAsync(int page, int pageSize, int? apartmentId, bool? isPaid, DateTime? dueBefore);

        Task<PaymentResponse?> GetPaymentByIdAsync(int id);

        Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request);

        Task<PaymentResponse?> UpdatePaymentAsync(int id, PaymentUpdateRequest request);

        Task<bool> DeletePaymentAsync(int id);

        Task<PaymentResponse> MarkPaymentAsPaidAsync(int paymentId);

        Task<PaymentResponse> MarkPaymentAsUnpaidAsync(int paymentId);
    }
}
