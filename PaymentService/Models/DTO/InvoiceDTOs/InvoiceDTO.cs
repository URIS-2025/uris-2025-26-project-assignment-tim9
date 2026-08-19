using PaymentService.Models.DTO.InvoiceItemDTOs;
using PaymentService.Models.Enums;

namespace PaymentService.Models.DTO.InvoiceDTOs
{
    public class InvoiceDTO
    {
        public Guid InvoiceId { get; set; }
        public Guid ProjectId { get; set; }
        public DateTime IssueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public InvoiceStatus Status { get; set; }
        public Guid IssuedByUserId { get; set; }
        public List<InvoiceItemDTO> Items { get; set; } = new List<InvoiceItemDTO>();
    }
}
