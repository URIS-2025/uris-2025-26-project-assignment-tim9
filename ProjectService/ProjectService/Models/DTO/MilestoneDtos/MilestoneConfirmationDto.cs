namespace ProjectService.Models.DTO.MilestoneDtos
{
    public class MilestoneConfirmationDto
    {
        public Guid MilestoneId { get; set; }
        public Guid ProjectId { get; set; }
        public DateTime ExpectedDate { get; set; }
    }
}
