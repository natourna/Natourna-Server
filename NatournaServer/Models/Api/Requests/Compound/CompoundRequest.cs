using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Api.Requests.Compound
{
    public class CompoundRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int ActiveApartments { get; set; }
    }
}
