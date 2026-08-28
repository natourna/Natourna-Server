using NatournaServer.Models.Api.Response.User;

namespace NatournaServer.Models.Api.Response.Login
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public UserResponse User { get; set; } = new();
    }
}
