using WorkPackageService.Models.Enums;

namespace WorkPackageService.Models.DTO.WorkPackageDTOs
{
    public class WorkPackageUpdateDTO
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public WorkPackageStatus? Status { get; set; }
    }
}
