using System.ComponentModel.DataAnnotations;

namespace ProjectService.Validation
{
    public class NotEmptyGuidAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is Guid guid && guid == Guid.Empty)
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.MemberName} must not be an empty Guid.");
            }
            return ValidationResult.Success;
        }
    }
}