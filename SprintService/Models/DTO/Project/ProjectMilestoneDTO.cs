namespace SprintService.Models.DTO.Project
{
    public class ProjectMilestoneDTO
    {
        public Guid MilestoneId { get; set; }
        public Guid ProjectId { get; set; }
        public DateTime ExpectedDate { get; set; }
    }
}
