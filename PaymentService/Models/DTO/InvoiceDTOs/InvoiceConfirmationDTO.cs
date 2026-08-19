using PaymentService.Models.Enums;

namespace PaymentService.Models.DTO.InvoiceDTOs
{
    //odgovor na POST i PUT, dopunjen nazivom projekta i korisnickim imenom izdavaoca
    public class InvoiceConfirmationDTO
    {
        public Guid InvoiceId { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string IssuedByUsername { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public InvoiceStatus Status { get; set; }
        public int ItemCount { get; set; }
    }
}
