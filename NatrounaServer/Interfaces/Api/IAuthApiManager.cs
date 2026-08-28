using NatrounaServer.Models.Api.Requests.Login;
using NatrounaServer.Models.Api.Response.Login;

namespace NatrounaServer.Interfaces.Api
{
    public interface IAuthApiManager
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);

        Task<LoginResponse?> RefreshTokenAsync(string username);

        bool ValidateToken(string token);
    }
}
