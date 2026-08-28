using System.Security.Claims;

namespace NatrounaServer.Interfaces.Authentication
{
    public interface IJwtAuthenticationService
    {
        string GenerateToken(string username, string userId, string role = "Admin");
        ClaimsPrincipal? ValidateToken(string token);
    }
}
