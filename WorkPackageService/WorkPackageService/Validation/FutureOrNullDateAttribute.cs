using System.ComponentModel.DataAnnotations;

namespace WorkPackageService.Validation
{
    public class FutureOrNullDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                if (date <= DateTime.Now)
                {
                    return new ValidationResult(ErrorMessage ?? $"{validationContext.MemberName} must be in the future.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
