using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.DTO.AuthDtos
{
    public class RefreshDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
