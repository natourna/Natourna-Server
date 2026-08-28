using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Api.Requests.Payment
{
    public class PaymentAllocationRequest
    {
        [Required]
        public int BalanceId { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal Percentage { get; set; }

        public PaymentAllocationRequest(int balanceId, decimal percentage)
        {
            BalanceId = balanceId;
            Percentage = percentage;
        }
    }
}
