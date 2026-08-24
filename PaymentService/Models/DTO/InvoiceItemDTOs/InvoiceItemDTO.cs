namespace PaymentService.Models.DTO.InvoiceItemDTOs
{
    public class InvoiceItemDTO
    {
        public Guid InvoiceItemId { get; set; }
        public Guid InvoiceId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
