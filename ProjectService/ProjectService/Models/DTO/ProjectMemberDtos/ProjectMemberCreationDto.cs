namespace ProjectService.Models.DTO.ProjectMemberDtos
{
    public class ProjectMemberCreationDto
    {
        public Guid ProjectId { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool Status { get; set; }
    }
}
