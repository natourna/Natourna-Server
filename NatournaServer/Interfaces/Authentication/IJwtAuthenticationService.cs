using System.Security.Claims;

namespace NatournaServer.Interfaces.Authentication
{
    public interface IJwtAuthenticationService
    {
        string GenerateToken(string username, string userId, string role, int organizationId);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
