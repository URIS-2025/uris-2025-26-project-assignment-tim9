using AttachmentService.Models.DTO.User;

namespace AttachmentService.ServiceCalls.User
{
    public interface IUserService
    {
        Task<UserInfoDTO?> GetUserInfoAsync(Guid userId);
    }
}
