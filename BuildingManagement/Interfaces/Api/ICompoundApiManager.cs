using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Api
{
    public interface ICompoundApiManager
    {
        Task<List<CompoundEntity>> GetAllCompoundsAsync();

        Task<CompoundEntity?> GetCompoundByIdAsync(int id);

        Task<CompoundEntity> CreateCompoundAsync(CompoundEntity compound);

        Task<CompoundEntity?> UpdateCompoundAsync(int id, CompoundEntity compound);

        Task<bool> DeleteCompoundAsync(int id);
    }
}