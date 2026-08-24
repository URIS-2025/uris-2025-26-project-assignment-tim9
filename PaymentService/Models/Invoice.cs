using PaymentService.Models.Enums;

namespace PaymentService.Models
{
    public class Invoice
    {
        public Guid InvoiceId { get; set; }

        //projekat za koji je faktura izdata
        public Guid ProjectId { get; set; }

        public DateTime IssueDate { get; set; }

        //zbir svih stavki, preracunava se u servisu pri svakoj izmeni stavki
        public decimal TotalAmount { get; set; }

        public InvoiceStatus Status { get; set; }

        //korisnik koji je izdao fakturu, stize kroz X-User-Id header
        public Guid IssuedByUserId { get; set; }

        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

        public List<Payment> Payments { get; set; } = new List<Payment>();
    }
}
