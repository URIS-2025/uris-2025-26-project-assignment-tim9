namespace WorkPackageService.Models.DTO.TaskDTOs
{
    public class TaskReassignResultDTO
    {
        public TaskDisplayDTO Task { get; set; } = null!;
        public Guid? OldAssigneeId { get; set; }
        public Guid NewAssigneeId { get; set; }
    }
}
