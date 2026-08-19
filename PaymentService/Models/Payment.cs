using PaymentService.Models.Enums;

namespace PaymentService.Models
{
    public class Payment
    {
        public Guid PaymentId { get; set; }

        public Guid InvoiceId { get; set; }

        //novcani iznosi su decimal zbog tacnog zaokruzivanja
        public decimal Amount { get; set; }

        public PaymentStatus Status { get; set; }

        public DateTime Date { get; set; }

        //korisnik koji je izvrsio uplatu, stize kroz X-User-Id header
        public Guid PaidByUserId { get; set; }

        //navigaciono svojstvo ka fakturi
        public Invoice? Invoice { get; set; }
    }
}
