using ProjectService.Models;
using ProjectService.Models.DTO.ProjectDtos;

namespace ProjectService.Data
{
    public interface IProjectRepository
    {
        IEnumerable<ProjectDto> GetProjects();
        IEnumerable<ProjectDto> GetProjectsByStatus(string status);
        IEnumerable<ProjectDto> GetProjectsByMemberId(Guid memberId);
        ProjectDto GetProjectById(Guid ProjectId);
        ProjectConfirmationDto CreateProject(ProjectCreationDto project);
        ProjectConfirmationDto UpdateProject(Project project);
        void DeleteProject(Guid ProjectId);
        bool SaveChanges();
    }
}
