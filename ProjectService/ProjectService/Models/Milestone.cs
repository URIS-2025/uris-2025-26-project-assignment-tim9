namespace ProjectService.Models
{
    public class Milestone
    {
        public Guid MilestoneID { get; set; }
        public Guid ProjectID { get; set; }
        public DateTime ExpectedDate { get; set; }
    }
}
