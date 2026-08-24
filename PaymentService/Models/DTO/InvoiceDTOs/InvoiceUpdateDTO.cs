using System.ComponentModel.DataAnnotations;
using PaymentService.Models.Enums;
using PaymentService.Validation;

namespace PaymentService.Models.DTO.InvoiceDTOs
{
    //telo PUT zahteva, stavke se menjaju preko InvoiceItem endpointa
    public class InvoiceUpdateDTO : IValidatableObject
    {
        public Guid? ProjectId { get; set; }

        [NotFutureDate(ErrorMessage = "Datum izdavanja fakture ne moze biti u buducnosti.")]
        public DateTime? IssueDate { get; set; }

        [EnumDataType(typeof(InvoiceStatus), ErrorMessage = "Nepoznat status fakture.")]
        public InvoiceStatus? Status { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ProjectId.HasValue && ProjectId.Value == Guid.Empty)
            {
                yield return new ValidationResult(
                    "ProjectId ne moze biti prazan GUID kada je prosledjen.",
                    new[] { nameof(ProjectId) });
            }
        }
    }
}
