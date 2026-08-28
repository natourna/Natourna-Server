using NatournaServer.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Api.Requests.Payment
{
    public class PaymentUpdateRequest
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [RequiredInt]
        public int ApartmentId { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? PaymentDate { get; set; }

        public bool IsPaid { get; set; }

        public int? CycleId { get; set; }
    }
}
