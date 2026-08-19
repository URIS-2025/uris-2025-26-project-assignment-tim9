namespace ProjectService.Models.DTO.ProjectMemberDtos
{
    public class ProjectMemberDto
    {
        public Guid ProjectMemberId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool Status { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
    }
}
