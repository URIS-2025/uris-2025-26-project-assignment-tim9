using PaymentService.Models.Enums;

namespace PaymentService.Models.DTO.PaymentDTOs
{
    public class PaymentDTO
    {
        public Guid PaymentId { get; set; }
        public Guid InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime Date { get; set; }
        public Guid PaidByUserId { get; set; }
    }
}
