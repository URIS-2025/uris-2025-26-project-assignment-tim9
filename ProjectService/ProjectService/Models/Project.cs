using ProjectService.Models.Enums;

namespace ProjectService.Models

{
    public class Project
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; }
        public int Budget { get; set; }
        public ProjectStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Deadline { get; set; }

    }
}
