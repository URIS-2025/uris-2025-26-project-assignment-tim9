using System.ComponentModel.DataAnnotations;

namespace TimelogService.Validation
{
    public class NoFutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime date && date > DateTime.Now)
            {
                var memberNames = validationContext.MemberName is null
                    ? Array.Empty<string>()
                    : new[] { validationContext.MemberName };

                return new ValidationResult(ErrorMessage ?? "Timelog date can not be in the future time.", memberNames);
            }

            return ValidationResult.Success;
        }
    }
}