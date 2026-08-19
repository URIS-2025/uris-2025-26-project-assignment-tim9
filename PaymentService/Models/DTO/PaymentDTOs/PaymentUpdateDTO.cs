using System.ComponentModel.DataAnnotations;
using PaymentService.Models.Enums;

namespace PaymentService.Models.DTO.PaymentDTOs
{
    //telo PUT zahteva, nullable polja - menja se samo ono sto je poslato
    public class PaymentUpdateDTO
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Iznos uplate mora biti veci od nule.")]
        public decimal? Amount { get; set; }

        [EnumDataType(typeof(PaymentStatus), ErrorMessage = "Nepoznat status uplate.")]
        public PaymentStatus? Status { get; set; }
    }
}
