using ProjectService.Models.Enums;

namespace ProjectService.Models.DTO.ProjectDtos
{
    public class ProjectConfirmationDto
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Budget { get; set; }
        public ProjectStatus Status { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}