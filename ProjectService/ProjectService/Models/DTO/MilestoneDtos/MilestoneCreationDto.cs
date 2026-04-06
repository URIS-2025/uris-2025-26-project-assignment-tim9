namespace ProjectService.Models.DTO.MilestoneDtos
{
    public class MilestoneCreationDto
    {
        public Guid ProjectId { get; set; }
        public DateTime ExpectedDate { get; set; }
    }
}
