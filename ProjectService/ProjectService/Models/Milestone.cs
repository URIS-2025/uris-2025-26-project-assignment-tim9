using System.ComponentModel.DataAnnotations;

namespace ProjectService.Models
{
    public class Milestone
    {
        [Key]
        public Guid MilestoneId { get; set; }
        public Guid ProjectId { get; set; }
        public DateTime ExpectedDate { get; set; }
    }
}
