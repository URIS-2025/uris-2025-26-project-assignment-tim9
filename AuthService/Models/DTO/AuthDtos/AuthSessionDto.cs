using AuthService.Models.Enums;

namespace AuthService.Models.DTO.AuthDtos
{
    public class AuthSessionDto
    {
        public Guid AuthId { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public UserRole Permission { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
    }
}
