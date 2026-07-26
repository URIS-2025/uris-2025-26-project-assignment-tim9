using System.ComponentModel.DataAnnotations;
using WorkPackageService.Models.Enums;
using WorkPackageService.Validation;
using TaskPriority = WorkPackageService.Models.Enums.TaskPriority;
using TaskStatus = WorkPackageService.Models.Enums.TaskStatus;

namespace WorkPackageService.Models.DTO.TaskDTOs
{
    public class TaskUpdateDTO
    {
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public TaskStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public Guid? AssigneeId { get; set; }
        public Guid? ApproverId { get; set; }

        [FutureOrNullDate]
        public DateTime? DueDate { get; set; }
    }
}
