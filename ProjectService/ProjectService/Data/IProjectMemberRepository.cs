using ProjectService.Models;
using ProjectService.Models.DTO.ProjectDtos;
using ProjectService.Models.DTO.ProjectMemberDtos;

namespace ProjectService.Data
{
    public interface IProjectMemberRepository
    {
        IEnumerable<ProjectMemberDto> GetProjectMembers();
        IEnumerable<ProjectMemberDto> GetMembersByProjectId(Guid ProjectId);
        ProjectMemberDto GetProjectMemberById(Guid ProjectMemberId);
        ProjectMemberConfirmationDto CreateProjectMember(ProjectMemberCreationDto projectMember);
        ProjectMemberConfirmationDto UpdateProjectMember(ProjectMember projectMember);
        void DeleteProjectMember(Guid ProjectMemberId);
        bool SaveChanges();
    }
}

