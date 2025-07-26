using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Context
{
    public interface IBillContextManager
    {
        Task<List<BillEntity>> GetAllAsync();

        Task<BillEntity?> GetByIdAsync(int id);

        Task<List<BillEntity>> GetByCompoundIdAsync(int compoundId);

        Task<BillEntity> CreateAsync(BillEntity bill);

        Task<BillEntity?> UpdateAsync(int id, BillEntity bill);

        Task<bool> DeleteAsync(int id);
    }
}