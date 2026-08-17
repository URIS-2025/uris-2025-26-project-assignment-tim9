using AttachmentService.Models.DTO.Project;

namespace AttachmentService.ServiceCalls.Project
{
    public interface IProjectService
    {
        Task<UserInfoDTO?> GetUserInfoAsync(Guid userId);
    }
}
