using System.ComponentModel.DataAnnotations;
using WorkPackageService.Validation;

namespace WorkPackageService.Models.DTO.BacklogDTOs
{
    public class BacklogCreateDTO
    {
        [NotEmptyGuid]
        public Guid ProjectId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [NotEmptyGuid]
        public Guid CreatedBy { get; set; }
    }
}
