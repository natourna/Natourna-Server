using System.ComponentModel.DataAnnotations;

namespace NatrounaServer.Models.Api.Requests.Login
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
