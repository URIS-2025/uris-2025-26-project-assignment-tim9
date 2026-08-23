using AuthService.Models.DTO.UserDtos;

namespace AuthService.ServiceCalls.User
{
    public interface IUserService
    {
        Task<UserAuthVo?> ValidateCredentialsAsync(string username, string password);
    }
}
