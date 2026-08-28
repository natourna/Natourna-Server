using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Api.Requests.Balance
{
    public class BalanceRequest
    {
        [Required]
        [MaxLength(100)]
        public string Label { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal CurrentAmount { get; set; }

        [Range(1, int.MaxValue)]
        public int CompoundId { get; set; }
    }
}
