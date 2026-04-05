namespace ProjectService.Models.DTO.ProjectMemberDtos
{
    public class ProjectMemberUpdateDto
    {
        public Guid ProjectMemberID { get; set; }
        public Guid ProjectID { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool Status { get; set; }
    }
}
