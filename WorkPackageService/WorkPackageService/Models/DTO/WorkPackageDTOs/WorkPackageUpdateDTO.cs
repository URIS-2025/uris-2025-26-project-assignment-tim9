using System.ComponentModel.DataAnnotations;
using WorkPackageService.Models.Enums;

namespace WorkPackageService.Models.DTO.WorkPackageDTOs
{
    public class WorkPackageUpdateDTO
    {
        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public WorkPackageStatus? Status { get; set; }
    }
}
