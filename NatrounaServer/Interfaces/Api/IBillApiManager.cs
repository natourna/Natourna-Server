using NatrounaServer.Models.Api.Requests.Bill;
using NatrounaServer.Models.Api.Response.Bill;
using NatrounaServer.Models.Entities;

namespace NatrounaServer.Interfaces.Api
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