using WorkPackageService.Models.Enums;

namespace WorkPackageService.Models.DTO.WorkPackageDTOs
{
    public class WorkPackageCreateDTO
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public WorkPackageStatus Status { get; set; }
    }
}
