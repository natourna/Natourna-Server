using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Api.Requests.Payment
{
    public class PaymentUpdateRequest
    {
        [Required]
        [MaxLength(200)]
        public string Label { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public DateTime? DueDate { get; set; }

        [Range(1, int.MaxValue)]
        public int ApartmentId { get; set; }
    }
}
