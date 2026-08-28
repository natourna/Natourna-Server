using System.ComponentModel.DataAnnotations;

namespace NatrounaServer.Models.Validation
{
    /// <summary>
    /// Validates that payment allocations sum to exactly 100%
    /// </summary>
    public class PaymentAllocationsValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not IEnumerable<dynamic> allocations)
            {
                return new ValidationResult("Allocations must be provided");
            }

            var allocationsList = allocations.ToList();

            if (!allocationsList.Any())
            {
                return new ValidationResult("At least one balance allocation is required");
            }

            decimal totalPercentage = 0;
            try
            {
                foreach (var allocation in allocationsList)
                {
                    totalPercentage += allocation.Percentage;
                }
            }
            catch
            {
                return new ValidationResult("Invalid allocation format");
            }

            if (totalPercentage != 100)
            {
                return new ValidationResult($"Allocations must sum to exactly 100%. Current sum: {totalPercentage}%");
            }

            return ValidationResult.Success;
        }
    }
}
