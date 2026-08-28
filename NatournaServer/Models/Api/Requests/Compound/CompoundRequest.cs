using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Api.Requests.Compound
{
    public class CompoundRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;
    }
}
