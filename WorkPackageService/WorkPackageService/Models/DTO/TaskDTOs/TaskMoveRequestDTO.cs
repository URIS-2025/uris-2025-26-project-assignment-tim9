using WorkPackageService.Validation;

namespace WorkPackageService.Models.DTO.TaskDTOs
{
    public class TaskMoveRequestDTO
    {
        [NotEmptyGuid]
        public Guid NewWorkPackageId { get; set; }
    }
}
