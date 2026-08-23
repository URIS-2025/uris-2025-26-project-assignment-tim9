using System.ComponentModel.DataAnnotations;
using ProjectService.Validation;

namespace ProjectService.Models.DTO.RequirementsDtos
{
    public class RequirementsUpdateDto
    {
        [NotEmptyGuid]
        public Guid RequirementId { get; set; }

        [NotEmptyGuid]
        public Guid ProjectId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;
    }
}