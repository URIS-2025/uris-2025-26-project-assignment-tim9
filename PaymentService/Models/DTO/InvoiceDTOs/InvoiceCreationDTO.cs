using System.ComponentModel.DataAnnotations;
using PaymentService.Models.DTO.InvoiceItemDTOs;
using PaymentService.Validation;

namespace PaymentService.Models.DTO.InvoiceDTOs
{
    //telo POST zahteva, faktura se izdaje zajedno sa stavkama
    public class InvoiceCreationDTO : IValidatableObject
    {
        [Required(ErrorMessage = "ProjectId je obavezan.")]
        public Guid ProjectId { get; set; }

        [Required(ErrorMessage = "Datum izdavanja je obavezan.")]
        [NotFutureDate(ErrorMessage = "Datum izdavanja fakture ne moze biti u buducnosti.")]
        public DateTime IssueDate { get; set; }

        [MinLength(1, ErrorMessage = "Faktura mora imati najmanje jednu stavku.")]
        public List<InvoiceItemCreationDTO> Items { get; set; } = new List<InvoiceItemCreationDTO>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ProjectId == Guid.Empty)
            {
                yield return new ValidationResult(
                    "ProjectId ne moze biti prazan GUID.",
                    new[] { nameof(ProjectId) });
            }
        }
    }
}
