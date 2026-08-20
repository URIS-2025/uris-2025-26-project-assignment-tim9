namespace ProjectService.Models.DTO.MilestoneDtos
{
    public class MilestoneDto
    {
        public Guid MilestoneId { get; set; }
        public Guid ProjectId { get; set; }
        public DateTime ExpectedDate { get; set; }
    }
}
