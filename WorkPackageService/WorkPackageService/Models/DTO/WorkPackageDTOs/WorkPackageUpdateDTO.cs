using System.ComponentModel.DataAnnotations;
using WorkPackageService.Models.Enums;
using WorkPackageService.Validation;

namespace WorkPackageService.Models.DTO.WorkPackageDTOs
{
    public class WorkPackageUpdateDTO
    {
        [NotEmptyGuid]
        public Guid Id { get; set; }

        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public WorkPackageStatus? Status { get; set; }

        public DateTime? Deadline { get; set; }
    }
}
