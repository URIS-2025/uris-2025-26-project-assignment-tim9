using WorkPackageService.Models.Enums;
using TaskStatus = WorkPackageService.Models.Enums.TaskStatus;

namespace WorkPackageService.Models
{
    public class Task
    {
        public Guid TaskId { get; set; }
        public Guid WorkPackageId { get; set; }
        public Guid? ParentTaskId { get; set; }
        // SprintId lives in SprintService's own database - plain scalar Guid, same treatment
        // as AssigneeId/ApproverId for UserService. No EF foreign key.
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

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Dependency> Dependencies { get; set; } = new List<Dependency>();
        public ICollection<Task> SubTasks { get; set; } = new List<Task>();
    }
}
