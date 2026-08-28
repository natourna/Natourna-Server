using NatournaServer.Constants.Cycle;
using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Api.Requests.Cycle
{
    public class CycleUpdateRequest
    {
        [Required]
        [MaxLength(200)]
        public string Label { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public PaymentCycle Cycle { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
