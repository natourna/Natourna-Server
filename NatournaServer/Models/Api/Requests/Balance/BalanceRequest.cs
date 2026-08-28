using NatournaServer.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Api.Requests.Balance
{
    public class BalanceRequest
    {
        [Required]
        [MaxLength(200)]
        public string Label { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal CurrentAmount { get; set; }

        [RequiredInt]
        public int CompoundId { get; set; }
    }
}
