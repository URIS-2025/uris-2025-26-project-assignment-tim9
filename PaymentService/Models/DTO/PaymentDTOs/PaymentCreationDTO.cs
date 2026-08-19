using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models.DTO.PaymentDTOs
{
    //telo POST zahteva za novu uplatu, datum i platioca postavlja servis
    public class PaymentCreationDTO : IValidatableObject
    {
        [Required(ErrorMessage = "InvoiceId je obavezan.")]
        public Guid InvoiceId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Iznos uplate mora biti veci od nule.")]
        public decimal Amount { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (InvoiceId == Guid.Empty)
            {
                yield return new ValidationResult(
                    "InvoiceId ne moze biti prazan GUID.",
                    new[] { nameof(InvoiceId) });
            }
        }
    }
}
