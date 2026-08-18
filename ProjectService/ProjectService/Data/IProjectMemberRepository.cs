using ProjectService.Models;
using ProjectService.Models.DTO.ProjectDtos;
using ProjectService.Models.DTO.ProjectMemberDtos;

namespace ProjectService.Data
{
    public interface IProjectMemberRepository
    {
        IEnumerable<ProjectMemberDto> GetProjectMembers();
        IEnumerable<ProjectMemberDto> GetMembersByProjectId(Guid projectId);
        ProjectMemberDto GetProjectMemberById(Guid projectMemberId);
        ProjectMemberConfirmationDto CreateProjectMember(ProjectMemberCreationDto projectMember);
        ProjectMemberConfirmationDto? UpdateProjectMember(ProjectMemberUpdateDto projectMember);
        void DeleteProjectMember(Guid projectMemberId);
        bool SaveChanges();
    }
}

