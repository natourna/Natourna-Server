using System.ComponentModel.DataAnnotations;

namespace BuildingManagement.Validation
{
    public class RequiredDecimalAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is decimal decimalValue)
                return decimalValue != 0m;
            return false;
        }
    }
}
