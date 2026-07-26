namespace WorkPackageService.Models.DTO.TaskDTOs
{
    public class TaskMoveResultDTO
    {
        public TaskDisplayDTO Task { get; set; } = null!;
        public bool HasDependencyWarning { get; set; }
        public string? Warning { get; set; }
    }
}
