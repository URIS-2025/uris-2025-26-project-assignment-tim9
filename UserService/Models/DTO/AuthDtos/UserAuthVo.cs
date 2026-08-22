namespace UserService.Models.DTO.AuthDtos
{
    // Vraća se ka AuthService-u nakon uspešne validacije kredencijala
    public class UserAuthVo
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsValid { get; set; }
    }
}
