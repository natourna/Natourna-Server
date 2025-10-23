using BuildingManagement.Models.Api.Requests.Bill;
using BuildingManagement.Models.Api.Response.Bill;
using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Api
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