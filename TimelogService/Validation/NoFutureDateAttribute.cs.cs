using System.ComponentModel.DataAnnotations;

namespace TimelogService.Validation
{
    public class NoFutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                if (date > DateTime.Now)
                {
                    return new ValidationResult(ErrorMessage ?? "Timelog date can not be in the future time.");
                }
            }

            return ValidationResult.Success;
        }
    }
}