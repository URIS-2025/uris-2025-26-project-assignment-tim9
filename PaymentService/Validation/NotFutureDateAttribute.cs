using System.ComponentModel.DataAnnotations;

namespace PaymentService.Validation
{
    //odbija datume posle danasnjeg dana
    public class NotFutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime date && date.Date > DateTime.Now.Date)
            {
                var memberNames = validationContext.MemberName is null
                    ? Array.Empty<string>()
                    : new[] { validationContext.MemberName };

                return new ValidationResult(ErrorMessage ?? "Datum ne moze biti u buducnosti.", memberNames);
            }

            return ValidationResult.Success;
        }
    }
}
