using TimelogService.Models.DTO.Project;

namespace TimelogService.ServiceCalls.Project
{
    public interface IProjectService
    {
        Task<UserInfoDTO?> GetUserInfoAsync(Guid userId);
    }
}
