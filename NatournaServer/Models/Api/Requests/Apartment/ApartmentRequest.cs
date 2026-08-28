using NatournaServer.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Api.Requests.Apartment
{
    public class ApartmentRequest
    {
        [Required]
        public string ApartmentInfo { get; set; } = string.Empty;

        public string? Owner { get; set; }

        public string? Tenant { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(0, int.MaxValue)]
        public int Floor { get; set; }

        [RequiredInt]
        public int BuildingId { get; set; }
    }
}
