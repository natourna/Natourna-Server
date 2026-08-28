using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Api.Requests.User
{
    public class UpdateUserRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MinLength(8)]
        [MaxLength(128)]
        public string? Password { get; set; }

        [Required]
        [MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
