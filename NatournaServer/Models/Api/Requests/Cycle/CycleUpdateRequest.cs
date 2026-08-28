using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Api.Requests.Cycle
{
    public class CycleUpdateRequest
    {
        [Required]
        [MaxLength(200)]
        public string Label { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
