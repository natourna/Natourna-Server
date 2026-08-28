using NatournaServer.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Api.Requests.User
{
    public class UpdateUserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MinLength(8)]
        public string? Password { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [RequiredInt]
        public int RoleId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
