namespace WorkPackageService.Models.DTO.Task
{
    public class TaskUpdateDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public Guid ApproverId { get; set; }
        public Guid? ParentTaskId { get; set; }
    }
}