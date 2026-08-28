using System.Security.Claims;

namespace NatournaServer.Interfaces.Authentication
{
    public interface IJwtAuthenticationService
    {
        string GenerateToken(string username, string userId, string role);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
