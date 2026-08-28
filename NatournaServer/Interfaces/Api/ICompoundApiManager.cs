using NatournaServer.Models.Api.Requests.Compound;
using NatournaServer.Models.Api.Response.Compound;

namespace NatournaServer.Interfaces.Api
{
    public interface ICompoundApiManager
    {
        Task<List<CompoundResponse>> GetAllCompoundsAsync();

        Task<CompoundResponse?> GetCompoundByIdAsync(int id);

        Task<CompoundResponse> CreateCompoundAsync(CompoundRequest request);

        Task<CompoundResponse?> UpdateCompoundAsync(int id, CompoundRequest request);

        Task<bool> DeleteCompoundAsync(int id);
    }
}
