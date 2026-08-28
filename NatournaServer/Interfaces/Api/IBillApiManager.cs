using NatournaServer.Models.Api.Requests.Bill;
using NatournaServer.Models.Api.Requests.Paging;
using NatournaServer.Models.Api.Response.Bill;
using NatournaServer.Models.Api.Response.Paging;

namespace NatournaServer.Interfaces.Api
{
    public interface IBillApiManager
    {
        Task<PagedResponse<BillResponse>> GetPagedBillsAsync(PagedQuery query, int? balanceId = null, bool? isPaid = null);

        Task<BillResponse?> GetBillByIdAsync(int id);

        Task<BillResponse> CreateBillAsync(BillRequest bill);

        Task<BillResponse?> UpdateBillAsync(int id, BillUpdateRequest bill);

        Task<bool> DeleteBillAsync(int id);

        Task<BillResponse> MarkBillAsPaidAsync(int billId);

        Task<BillResponse> MarkBillAsUnpaidAsync(int billId);
    }
}