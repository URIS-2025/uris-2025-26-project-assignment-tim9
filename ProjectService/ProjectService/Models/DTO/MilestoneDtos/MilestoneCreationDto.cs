namespace ProjectService.Models.DTO.MilestoneDtos
{
    public class MilestoneCreationDto
    {
        public Guid ProjectID { get; set; }
        public DateTime ExpectedDate { get; set; }
    }
}
