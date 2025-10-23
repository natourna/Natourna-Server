using BuildingManagement.Models.Api.Requests.Login;
using BuildingManagement.Models.Api.Response.Login;

namespace BuildingManagement.Interfaces.Api
{
    public interface IAuthApiManager
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);

        Task<LoginResponse?> RefreshTokenAsync(string username);

        bool ValidateToken(string token);
    }
}
