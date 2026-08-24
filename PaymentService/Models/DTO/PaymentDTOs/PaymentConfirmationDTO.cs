using PaymentService.Models.Enums;

namespace PaymentService.Models.DTO.PaymentDTOs
{
    //odgovor na POST i PUT, dopunjen podacima iz User i Project servisa
    public class PaymentConfirmationDTO
    {
        public Guid PaymentId { get; set; }
        public Guid InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime Date { get; set; }
        public string PaidByUsername { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
    }
}
