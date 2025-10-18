using System.ComponentModel.DataAnnotations;
using BuildingManagement.Validation;

namespace BuildingManagement.Models.Requests.Payment
{
    public class PaymentRequest
    {
        [Required]
        public int ApartmentId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [PaymentAllocationsValidation(ErrorMessage = "Allocations must sum to exactly 100%")]
        public List<PaymentAllocationRequest> Allocations { get; set; }

        public DateTime? PaymentDate { get; set; }

        public DateTime? DueDate { get; set; }

        public bool IsPaid { get; set; } = false;

        public PaymentRequest(int apartmentId, decimal amount, List<PaymentAllocationRequest> allocations)
        {
            ApartmentId = apartmentId;
            Amount = amount;
            Allocations = allocations;
        }
    }
}