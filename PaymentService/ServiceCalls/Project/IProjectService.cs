using PaymentService.Models.DTO.Project;

namespace PaymentService.ServiceCalls.Project
{
    public interface IProjectService
    {
        Task<ProjectInfoDTO?> GetProjectInfoAsync(Guid projectId);
    }
}
