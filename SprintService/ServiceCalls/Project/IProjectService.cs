using SprintService.Models.DTO.Project;

namespace SprintService.ServiceCalls.Project
{
    public interface IProjectService
    {
        Task<MilestoneDTO?> GetProjectByIdAsync(Guid id);
        Task<ProjectExistence> CheckProjectExistsAsync(Guid id);
        Task<List<Guid>> GetProjectIdsForUserAsync(Guid userId);
    }
}
