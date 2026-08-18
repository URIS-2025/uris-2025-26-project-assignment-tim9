using ProjectService.Validation;

namespace ProjectService.Models.DTO.ProjectMemberDtos
{
    public class ProjectMemberCreationDto
    {
        [NotEmptyGuid]
        public Guid ProjectId { get; set; }

        [NotEmptyGuid]
        public Guid UserId { get; set; }
    }
}