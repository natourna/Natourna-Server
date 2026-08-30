using NatournaServer.Models.Api.Requests.Paging;
using NatournaServer.Models.Api.Requests.Payment;
using NatournaServer.Models.Api.Response.Paging;
using NatournaServer.Models.Api.Response.Payment;

namespace NatournaServer.Interfaces.Api
{
    public interface IPaymentApiManager
    {
        Task<PagedResponse<PaymentResponse>> GetPagedPaymentsAsync(PagedQuery query, int? apartmentId = null, int? cycleId = null, bool? isPaid = null, bool? overdue = null);

        Task<PaymentResponse?> GetPaymentByIdAsync(int id);

        Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request);

        Task<PaymentResponse?> UpdatePaymentAsync(int id, PaymentUpdateRequest payment);

        Task<bool> DeletePaymentAsync(int id);

        Task<PaymentResponse> MarkPaymentAsPaidAsync(int paymentId);

        Task<PaymentResponse> MarkPaymentAsUnpaidAsync(int paymentId);
    }
}
