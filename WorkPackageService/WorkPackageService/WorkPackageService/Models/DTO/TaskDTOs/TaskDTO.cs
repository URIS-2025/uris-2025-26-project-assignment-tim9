namespace WorkPackageService.Models.DTO.Task
{
    public class TaskDTO
    {
        public Guid TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid ApproverId { get; set; }
        public Guid WorkPackageId { get; set; }
        public Guid? ParentTaskId { get; set; }
    }
}