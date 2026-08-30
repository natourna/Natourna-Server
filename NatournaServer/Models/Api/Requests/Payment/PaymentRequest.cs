using NatournaServer.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Api.Requests.Payment
{
    public class PaymentRequest
    {
        [Required]
        public int ApartmentId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Label { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [PaymentAllocationsValidation(ErrorMessage = "Allocations must sum to exactly 100%")]
        public List<PaymentAllocationRequest> Allocations { get; set; }

        public DateTime? DueDate { get; set; }

        public PaymentRequest(int apartmentId, string label, decimal amount, List<PaymentAllocationRequest> allocations)
        {
            ApartmentId = apartmentId;
            Label = label;
            Amount = amount;
            Allocations = allocations;
        }
    }
}