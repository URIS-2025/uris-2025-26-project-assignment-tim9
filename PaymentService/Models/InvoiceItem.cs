namespace PaymentService.Models
{
    public class InvoiceItem
    {
        public Guid InvoiceItemId { get; set; }

        public Guid InvoiceId { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        //UnitPrice * Quantity, racuna se u servisu
        public decimal TotalAmount { get; set; }

        //navigaciono svojstvo ka fakturi
        public Invoice? Invoice { get; set; }
    }
}
