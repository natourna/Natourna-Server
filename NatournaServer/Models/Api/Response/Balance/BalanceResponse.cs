namespace NatournaServer.Models.Api.Response.Balance
{
    public class BalanceResponse
    {
        public int Id { get; set; }

        public string Label { get; set; } = string.Empty;

        public decimal CurrentAmount { get; set; }

        public int CompoundId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
