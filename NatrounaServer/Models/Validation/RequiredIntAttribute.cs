using System.ComponentModel.DataAnnotations;

namespace NatrounaServer.Models.Validation
{
    public class RequiredIntAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is int intValue)
                return intValue != 0;
            return false;
        }
    }
}
