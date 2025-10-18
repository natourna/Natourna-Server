using System.ComponentModel.DataAnnotations;
using BuildingManagement.Constants.Cycle;
using BuildingManagement.Models.Requests.Payment;
using BuildingManagement.Validation;

namespace BuildingManagement.Models.Requests.Cycle
{
    /// <summary>
    /// Request model for creating a payment cycle
    /// </summary>
    public class CycleRequest
    {
        [Required]
        public string Label { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public PaymentCycle Cycle { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// List of apartment IDs to include in this cycle.
        /// If null or empty, cycle applies to ALL apartments in the compound.
        /// </summary>
        public List<int>? ApartmentIds { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Balance allocations for payments created by this cycle.
        /// Percentages must sum to 100%.
        /// REQUIRED - All payments must have balance allocations.
        /// </summary>
        [Required]
        [PaymentAllocationsValidation(ErrorMessage = "Balance allocations must sum to exactly 100%")]
        public List<PaymentAllocationRequest> BalanceAllocations { get; set; } = new();

        public CycleRequest() { }

        public CycleRequest(string label, PaymentCycle cycle, DateTime startDate, DateTime endDate, decimal amount, List<PaymentAllocationRequest> balanceAllocations, string? description = null, List<int>? apartmentIds = null)
        {
            Label = label;
            Description = description;
            Cycle = cycle;
            StartDate = startDate;
            EndDate = endDate;
            ApartmentIds = apartmentIds;
            Amount = amount;
            BalanceAllocations = balanceAllocations;
        }
    }
}
