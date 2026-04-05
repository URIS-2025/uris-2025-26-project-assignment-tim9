namespace ProjectService.Models.DTO.ProjectMemberDtos
{
    public class ProjectMemberCreationDt
    {
        public Guid ProjectID { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool Status { get; set; }
    }
}
