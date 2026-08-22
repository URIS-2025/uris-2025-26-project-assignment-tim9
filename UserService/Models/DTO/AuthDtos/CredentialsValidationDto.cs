using System.ComponentModel.DataAnnotations;

namespace UserService.Models.DTO.AuthDtos
{
    public class CredentialsValidationDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
