namespace AuthService.Models.DTO.UserDtos
{
    // Odgovor UserService-a nakon provere kredencijala
    public class UserAuthVo
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsValid { get; set; }
    }
}
