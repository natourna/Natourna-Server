using BuildingManagement.Constants.Cycle;
using BuildingManagement.Models.Api.Requests.Payment;
using BuildingManagement.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace BuildingManagement.Models.Api.Requests.Cycle
{
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

        public List<int>? ApartmentIds { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

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
