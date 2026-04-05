namespace ProjectService.Models.DTO.ProjectMemberDtos
{
    public class ProjectMemberConfirmationDto
    {
        public Guid ProjectMemberID { get; set; }
        public Guid ProjectID { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool Status { get; set; }
    }
}
