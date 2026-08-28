using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Api.Requests.Building
{
    public class BuildingRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int NumberOfApartments { get; set; }

        [Range(1, 200)]
        public int Floors { get; set; }

        [Range(1, int.MaxValue)]
        public int CompoundId { get; set; }
    }
}
