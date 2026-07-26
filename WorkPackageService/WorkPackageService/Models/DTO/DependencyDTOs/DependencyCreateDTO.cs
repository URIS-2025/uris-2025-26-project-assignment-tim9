using WorkPackageService.Validation;

namespace WorkPackageService.Models.DTO.DependencyDTOs
{
    public class DependencyCreateDTO
    {
        [NotEmptyGuid]
        public Guid TaskId { get; set; }

        [NotEmptyGuid]
        [NotEqualToProperty(nameof(TaskId))]
        public Guid BlockerTaskId { get; set; }
    }
}
