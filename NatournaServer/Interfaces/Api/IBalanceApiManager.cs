using NatournaServer.Models.Api.Requests.Balance;
using NatournaServer.Models.Api.Response.Balance;

namespace NatournaServer.Interfaces.Api
{
    public interface IBalanceApiManager
    {
        Task<List<BalanceResponse>> GetAllBalancesAsync();

        Task<BalanceResponse?> GetBalanceByIdAsync(int id);

        Task<List<BalanceResponse>> GetBalancesByCompoundIdAsync(int compoundId);

        Task<BalanceResponse> CreateBalanceAsync(BalanceRequest balance);

        Task<BalanceResponse?> UpdateBalanceAsync(int id, BalanceRequest balance);

        Task<bool> DeleteBalanceAsync(int id);
    }
}
