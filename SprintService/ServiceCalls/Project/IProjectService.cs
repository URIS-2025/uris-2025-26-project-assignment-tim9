using SprintService.Models.DTO.Project;

namespace SprintService.ServiceCalls.Project
{
    public interface IProjectService
    {
        ProjectDTO GetProjectById(Guid id);
    }
}
