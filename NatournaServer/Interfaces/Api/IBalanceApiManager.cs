using NatournaServer.Models.Api.Requests.Balance;
using NatournaServer.Models.Api.Response.Balance;

namespace NatournaServer.Interfaces.Api
{
    public interface IBalanceApiManager
    {
        Task<List<BalanceResponse>> GetAllBalancesAsync(int? compoundId);

        Task<BalanceResponse?> GetBalanceByIdAsync(int id);

        Task<BalanceResponse> CreateBalanceAsync(BalanceRequest request);

        Task<BalanceResponse?> UpdateBalanceAsync(int id, BalanceRequest request);

        Task<bool> DeleteBalanceAsync(int id);
    }
}
