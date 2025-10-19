using System.Security.Claims;

namespace BuildingManagement.Interfaces.Authentication
{
    public interface IJwtAuthenticationService
    {
        string GenerateToken(string username, string role = "Admin");
        ClaimsPrincipal? ValidateToken(string token);
    }
}
