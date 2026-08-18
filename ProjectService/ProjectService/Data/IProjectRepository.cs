using ProjectService.Models;
using ProjectService.Models.DTO.ProjectDtos;
using ProjectService.Models.Enums;

namespace ProjectService.Data
{
    public interface IProjectRepository
    {
        IEnumerable<ProjectDto> GetProjects();
        IEnumerable<ProjectDto> GetProjectsByStatus(ProjectStatus status);
        IEnumerable<ProjectDto> GetProjectsByMemberId(Guid memberId);
        IEnumerable<ProjectDto> GetProjectsByUserId(Guid userId);
        ProjectDto GetProjectById(Guid ProjectId);
        ProjectConfirmationDto CreateProject(ProjectCreationDto project);
        ProjectConfirmationDto UpdateProject(ProjectUpdateDto project);
        void DeleteProject(Guid ProjectId);
        bool SaveChanges();
    }
}
