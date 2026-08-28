using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Validation
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
