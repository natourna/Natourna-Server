using BuildingManagement.Interfaces.Api;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Models.Entities;

namespace BuildingManagement.Services.Api
{
    public class CompoundApiManager : ICompoundApiManager
    {
        private readonly ICompoundContextManager _contextManager;

        public CompoundApiManager(ICompoundContextManager contextManager)
        {
            _contextManager = contextManager;
        }

        public async Task<List<CompoundEntity>> GetAllCompoundsAsync()
        {
            return await _contextManager.GetAllAsync();
        }

        public async Task<CompoundEntity?> GetCompoundByIdAsync(int id)
        {
            return await _contextManager.GetByIdAsync(id);
        }

        public async Task<CompoundEntity> CreateCompoundAsync(CompoundEntity compound)
        {
            return await _contextManager.CreateAsync(compound);
        }

        public async Task<CompoundEntity?> UpdateCompoundAsync(int id, CompoundEntity compound)
        {
            return await _contextManager.UpdateAsync(id, compound);
        }

        public async Task<bool> DeleteCompoundAsync(int id)
        {
            return await _contextManager.DeleteAsync(id);
        }
    }
}