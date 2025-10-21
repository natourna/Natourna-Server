namespace BuildingManagement.Models.Api.Response.Bill
{
    public class BillResponse
    {
        public int Id { get; set; }

        public string? Label { get; set; }

        public decimal Amount { get; set; }

        public DateTime? DueDate { get; set; }

        public bool IsPaid { get; set; } = false;

        public DateTime? PaymentDate { get; set; }

        public int BalanceId { get; set; }

        public string? BalanceName { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
