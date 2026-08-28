using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Api.Requests.Apartment
{
    public class ApartmentRequest
    {
        [Required]
        [MaxLength(100)]
        public string ApartmentInfo { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Owner { get; set; }

        [MaxLength(255)]
        public string? Tenant { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        [MaxLength(50)]
        public string Floor { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int BuildingId { get; set; }
    }
}
