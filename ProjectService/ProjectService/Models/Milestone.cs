using System.ComponentModel.DataAnnotations;

namespace ProjectService.Models
{
    public class Milestone
    {

        public Guid MilestoneId { get; set; }
        public Guid ProjectId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime ExpectedDate { get; set; }
    }
}
