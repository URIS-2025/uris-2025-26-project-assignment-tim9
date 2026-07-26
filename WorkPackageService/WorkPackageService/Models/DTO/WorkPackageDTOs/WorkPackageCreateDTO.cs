using System.ComponentModel.DataAnnotations;
using WorkPackageService.Models.Enums;
using WorkPackageService.Validation;

namespace WorkPackageService.Models.DTO.WorkPackageDTOs
{
    public class WorkPackageCreateDTO
    {
        [NotEmptyGuid]
        public Guid ProjectId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        public WorkPackageStatus Status { get; set; }
    }
}
