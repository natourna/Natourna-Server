using System.Security.Claims;

namespace NatournaServer.Interfaces.Authentication
{
    public interface IJwtAuthenticationService
    {
        string GenerateToken(string username, string userId, string role = "Admin");
        ClaimsPrincipal? ValidateToken(string token);
    }
}
