using SprintService.Models.DTO.Project;

namespace SprintService.ServiceCalls.Project
{
    public interface IProjectService
    {
        Task<MilestoneDTO?> GetProjectByIdAsync(Guid id);
    }
}
