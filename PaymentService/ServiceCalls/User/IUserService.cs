using PaymentService.Models.DTO.User;

namespace PaymentService.ServiceCalls.User
{
    public interface IUserService
    {
        Task<UserInfoDTO?> GetUserInfoAsync(Guid userId);
    }
}
