using WorkPackageService.Validation;

namespace WorkPackageService.Models.DTO.TaskDTOs
{
    public class TaskReassignRequestDTO
    {
        [NotEmptyGuid]
        public Guid NewAssigneeId { get; set; }
    }
}
