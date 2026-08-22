namespace AuthService.Models.DTO.UserDtos
{
    // Salje se ka UserService-u radi provere kredencijala
    public class CredentialsValidationDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
