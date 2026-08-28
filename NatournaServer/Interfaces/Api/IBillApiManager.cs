using NatournaServer.Models.Api.Requests.Bill;
using NatournaServer.Models.Api.Response.Bill;
using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Api
{
    public interface IBillApiManager
    {
        Task<List<BillResponse>> GetAllBillsAsync();

        Task<BillResponse?> GetBillByIdAsync(int id);

        Task<BillResponse> CreateBillAsync(BillRequest bill);

        Task<BillResponse?> UpdateBillAsync(int id, BillEntity bill);

        Task<bool> DeleteBillAsync(int id);

        Task<BillResponse> MarkBillAsPaidAsync(int billId);

        Task<BillResponse> MarkBillAsUnpaidAsync(int billId);
    }
}