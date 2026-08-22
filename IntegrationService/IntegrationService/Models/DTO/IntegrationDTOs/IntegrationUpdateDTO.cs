using System.ComponentModel.DataAnnotations;

namespace IntegrationService.Models.DTO.IntegrationDTOs
{
    public class IntegrationUpdateDTO
    {
        [Required]
        [MaxLength(100)]
        public string Type { get; set; } = string.Empty;

        // Opciono - ako se posalje, rotira se API kljuc. Ako je prazno, zadrzava se postojeci.
        public string? ApiKey { get; set; }

        [Required]
        public bool Status { get; set; }
    }
}
