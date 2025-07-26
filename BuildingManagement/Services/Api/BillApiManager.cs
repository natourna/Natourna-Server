using BuildingManagement.Interfaces.Api;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Models.Entities;

namespace BuildingManagement.Services.Api
{
    public class BillApiManager : IBillApiManager
    {
        private readonly IBillContextManager _contextManager;

        public BillApiManager(IBillContextManager contextManager)
        {
            _contextManager = contextManager;
        }

        public async Task<List<BillEntity>> GetAllBillsAsync()
        {
            return await _contextManager.GetAllAsync();
        }

        public async Task<BillEntity?> GetBillByIdAsync(int id)
        {
            return await _contextManager.GetByIdAsync(id);
        }

        public async Task<List<BillEntity>> GetBillsByCompoundIdAsync(int compoundId)
        {
            return await _contextManager.GetByCompoundIdAsync(compoundId);
        }

        public async Task<BillEntity> CreateBillAsync(BillEntity bill)
        {
            return await _contextManager.CreateAsync(bill);
        }

        public async Task<BillEntity?> UpdateBillAsync(int id, BillEntity bill)
        {
            return await _contextManager.UpdateAsync(id, bill);
        }

        public async Task<bool> DeleteBillAsync(int id)
        {
            return await _contextManager.DeleteAsync(id);
        }
    }
}