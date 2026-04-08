using TimelogService.Models.DTO.Project;

namespace TimelogService.ServiceCalls.Project
{
    public interface IProjectService
    {
        ProjectDTO GetProjectById(Guid id);
    }
}
