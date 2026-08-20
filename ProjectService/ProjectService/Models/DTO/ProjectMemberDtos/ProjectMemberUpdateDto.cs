using System.ComponentModel.DataAnnotations;
using ProjectService.Validation;

namespace ProjectService.Models.DTO.ProjectMemberDtos
{
    public class ProjectMemberUpdateDto
    {
        [NotEmptyGuid]
        public Guid ProjectMemberId { get; set; }

        [NotEmptyGuid]
        public Guid ProjectId { get; set; }

        [NotEmptyGuid]
        public Guid UserId { get; set; }

        public DateTime JoinedAt { get; set; }

        public bool Status { get; set; }
    }
}