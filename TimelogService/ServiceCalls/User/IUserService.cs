using TimelogService.Models.DTO.User;

namespace TimelogService.ServiceCalls.User
{
    public interface IUserService
    {
        Task<UserInfoDTO?> GetUserInfoAsync(Guid userId);
    }
}
