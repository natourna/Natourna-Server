using NatournaServer.Models.Api.Requests.Bill;
using NatournaServer.Models.Api.Response.Bill;
using NatournaServer.Models.Api.Response.Paging;

namespace NatournaServer.Interfaces.Api
{
    public interface IBillApiManager
    {
        Task<PagedResponse<BillResponse>> GetBillsAsync(int page, int pageSize, bool? isPaid);

        Task<BillResponse?> GetBillByIdAsync(int id);

        Task<BillResponse> CreateBillAsync(BillRequest request);

        Task<BillResponse?> UpdateBillAsync(int id, BillUpdateRequest request);

        Task<bool> DeleteBillAsync(int id);

        Task<BillResponse> MarkBillAsPaidAsync(int billId);

        Task<BillResponse> MarkBillAsUnpaidAsync(int billId);
    }
}
