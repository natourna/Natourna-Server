using NatournaServer.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Api.Requests.Organization
{
    /// <summary>
    /// Self-service signup: one call creates the organization, its trial subscription,
    /// the admin user, one compound and its buildings (optionally pre-filled with apartments).
    /// When CompoundName is omitted and there is exactly one building, the compound takes
    /// the building's name - the single-building customer case.
    /// </summary>
    public class RegisterOrganizationRequest
    {
        [Required]
        [MaxLength(200)]
        public string OrganizationName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string AdminEmail { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string AdminPassword { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? AdminPhoneNumber { get; set; }

        [MaxLength(200)]
        public string? CompoundName { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one building is required")]
        public List<RegisterBuildingRequest> Buildings { get; set; } = [];
    }

    public class RegisterBuildingRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [RequiredInt]
        [Range(1, 100)]
        public int Floors { get; set; }

        /// <summary>
        /// When greater than zero, Floors x ApartmentsPerFloor apartments are created
        /// (floors numbered from 0), ready to rename in the app.
        /// </summary>
        [Range(0, 20)]
        public int ApartmentsPerFloor { get; set; }
    }
}
