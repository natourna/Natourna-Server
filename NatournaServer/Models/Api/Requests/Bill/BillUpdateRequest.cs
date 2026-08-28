using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Api.Requests.Bill
{
    public class BillUpdateRequest
    {
        [Required]
        [MaxLength(200)]
        public string Label { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        public DateTime? DueDate { get; set; }

        public bool IsPaid { get; set; }

        public DateTime? PaymentDate { get; set; }
    }
}
