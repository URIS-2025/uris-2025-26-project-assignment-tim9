using System.ComponentModel.DataAnnotations;

namespace UserService.Models.DTO.UserDtos
{
    public class UserUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(50)]
        public string ContactInfo { get; set; } = string.Empty;
    }
}
