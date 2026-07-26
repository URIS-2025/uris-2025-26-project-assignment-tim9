using WorkPackageService.Models.Enums;

namespace WorkPackageService.Models.DTO.WorkPackageDTOs
{
    public class WorkPackageDisplayDTO
    {
        public Guid WorkPackageId { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public WorkPackageStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
