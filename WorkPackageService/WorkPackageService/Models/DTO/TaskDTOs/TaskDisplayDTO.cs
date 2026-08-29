using WorkPackageService.Models.Enums;
using TaskPriority = WorkPackageService.Models.Enums.TaskPriority;
using TaskStatus = WorkPackageService.Models.Enums.TaskStatus;

namespace WorkPackageService.Models.DTO.TaskDTOs
{
    public class TaskDisplayDTO
    {
        public Guid TaskId { get; set; }
        public Guid WorkPackageId { get; set; }
        public Guid? ParentTaskId { get; set; }
        public Guid? SprintId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public Guid? AssigneeId { get; set; }
        public Guid? ApproverId { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
