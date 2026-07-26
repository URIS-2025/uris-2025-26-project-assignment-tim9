using System.ComponentModel.DataAnnotations;

namespace WorkPackageService.Validation
{
    public class NotEqualToPropertyAttribute : ValidationAttribute
    {
        private readonly string _otherPropertyName;

        public NotEqualToPropertyAttribute(string otherPropertyName)
        {
            _otherPropertyName = otherPropertyName;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var otherProperty = validationContext.ObjectType.GetProperty(_otherPropertyName);
            if (otherProperty == null)
            {
                return new ValidationResult($"Unknown property '{_otherPropertyName}' referenced by NotEqualToProperty.");
            }

            var otherValue = otherProperty.GetValue(validationContext.ObjectInstance);

            if (value != null && value.Equals(otherValue))
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.MemberName} must not be equal to {_otherPropertyName}.");
            }

            return ValidationResult.Success;
        }
    }
}
