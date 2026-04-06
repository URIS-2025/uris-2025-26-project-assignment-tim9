using WorkPackageService.Models.Enums;

namespace WorkPackageService.Models
{
    public class Task
    {
        public Guid TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid ApproverId { get; set; }
        public Guid WorkPackageId { get; set; }
        public WorkPackage WorkPackage { get; set; }
        public Guid? ParentTaskId { get; set; }
        public Task ParentTask { get; set; }
        public ICollection<Task> SubTasks { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Dependency> Dependencies { get; set; }
    }
}