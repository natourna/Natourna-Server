using NatournaServer.Models.Api.Requests.Login;
using NatournaServer.Models.Api.Response.Login;

namespace NatournaServer.Interfaces.Api
{
    public interface IAuthApiManager
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);

        Task<LoginResponse?> RefreshTokenAsync(string username);

        bool ValidateToken(string token);
    }
}
