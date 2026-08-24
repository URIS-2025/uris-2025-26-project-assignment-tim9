namespace PaymentService.Models.DTO.User
{
    //podskup UserDto iz User servisa, samo ono sto nam treba
    public class UserInfoDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
