using NatournaServer.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Api.Requests.Building
{
    public class BuildingRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int NumberOfApartments { get; set; }

        [Range(1, int.MaxValue)]
        public int Floors { get; set; }

        [RequiredInt]
        public int CompoundId { get; set; }
    }
}
