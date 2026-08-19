namespace PaymentService.Models.DTO.InvoiceItemDTOs
{
    //odgovor na POST i PUT, vraca i novi ukupan iznos fakture
    public class InvoiceItemConfirmationDTO
    {
        public Guid InvoiceItemId { get; set; }
        public Guid InvoiceId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal InvoiceTotalAmount { get; set; }
    }
}
