using System.ComponentModel.DataAnnotations;

namespace BuildingManagement.Models.Validation
{
    public class RequiredEnumAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            var type = value.GetType();
            if (!type.IsEnum) return false;
            var intValue = (int)value;
            return intValue != 0;
        }
    }
}
