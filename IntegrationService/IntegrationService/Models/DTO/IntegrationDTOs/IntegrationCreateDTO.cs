using System.ComponentModel.DataAnnotations;

namespace IntegrationService.Models.DTO.IntegrationDTOs
{
    public class IntegrationCreateDTO
    {
        [Required]
        [MaxLength(100)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "API kljuc mora imati bar 8 karaktera.")]
        public string ApiKey { get; set; } = string.Empty;
    }
}
