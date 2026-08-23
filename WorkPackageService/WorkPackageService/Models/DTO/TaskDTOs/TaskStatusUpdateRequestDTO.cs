using TaskStatus = WorkPackageService.Models.Enums.TaskStatus;

namespace WorkPackageService.Models.DTO.TaskDTOs
{
    public class TaskStatusUpdateRequestDTO
    {
        public TaskStatus NewStatus { get; set; }
    }
}
