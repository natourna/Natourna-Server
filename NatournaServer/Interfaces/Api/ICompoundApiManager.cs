using NatournaServer.Models.Api.Requests.Compound;
using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Api
{
    public interface ICompoundApiManager
    {
        Task<List<CompoundEntity>> GetAllCompoundsAsync();

        Task<CompoundEntity?> GetCompoundByIdAsync(int id);

        Task<CompoundEntity> CreateCompoundAsync(CompoundRequest compound);

        Task<CompoundEntity?> UpdateCompoundAsync(int id, CompoundRequest compound);

        Task<bool> DeleteCompoundAsync(int id);
    }
}