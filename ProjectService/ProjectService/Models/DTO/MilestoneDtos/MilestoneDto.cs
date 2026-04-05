namespace ProjectService.Models.DTO.MilestoneDtos
{
    public class MilestoneDto
    {
        public Guid MilestoneID { get; set; }
        public Guid ProjectID { get; set; }
        public DateTime ExpectedDate { get; set; }
    }
}
