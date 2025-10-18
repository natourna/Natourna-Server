using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Api
{
    public interface IBillApiManager
    {
        Task<List<BillEntity>> GetAllBillsAsync();

        Task<BillEntity?> GetBillByIdAsync(int id);

        Task<List<BillEntity>> GetBillsByCompoundIdAsync(int compoundId);

        Task<BillEntity> CreateBillAsync(BillEntity bill);

        Task<BillEntity?> UpdateBillAsync(int id, BillEntity bill);

        Task<bool> DeleteBillAsync(int id);

        Task<BillEntity> MarkBillAsPaidAsync(int billId);

        Task<BillEntity> MarkBillAsUnpaidAsync(int billId);
    }
}