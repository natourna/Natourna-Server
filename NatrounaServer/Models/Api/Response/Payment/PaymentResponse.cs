namespace NatrounaServer.Models.Api.Response.Payment
{
    public class PaymentResponse
    {
        public int Id { get; set; }

        public string? Label { get; set; }

        public decimal Amount { get; set; }

        public DateTime? PaymentDate { get; set; }

        public DateTime? DueDate { get; set; }

        public bool IsPaid { get; set; }

        public int ApartmentId { get; set; }

        public string? ApartmentOwner { get; set; }

        public string? ApartmentTenant { get; set; }

        public int? CycleId { get; set; }

        public string? CycleName { get; set; }

        public bool Recurrent { get; set; }

        public string? PaymentOccurrence { get; set; }

        public List<PaymentAllocationResponse>? Allocations { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
