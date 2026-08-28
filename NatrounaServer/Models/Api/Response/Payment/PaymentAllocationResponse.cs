namespace NatrounaServer.Models.Api.Response.Payment
{
    public class PaymentAllocationResponse
    {
        public int Id { get; set; }

        public int PaymentId { get; set; }

        public int BalanceId { get; set; }

        public string? BalanceName { get; set; }

        public decimal Percentage { get; set; }

        public decimal AllocatedAmount { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
