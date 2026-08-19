using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models.DTO.InvoiceItemDTOs
{
    //telo PUT zahteva, nullable polja - menja se samo ono sto je poslato
    public class InvoiceItemUpdateDTO
    {
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Opis stavke mora imati izmedju 2 i 200 karaktera.")]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Jedinicna cena mora biti veca od nule.")]
        public decimal? UnitPrice { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Kolicina mora biti najmanje 1.")]
        public int? Quantity { get; set; }
    }
}
