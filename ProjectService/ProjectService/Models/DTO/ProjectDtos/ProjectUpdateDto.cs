using System.ComponentModel.DataAnnotations;
using ProjectService.Models.Enums;
using ProjectService.Validation;

namespace ProjectService.Models.DTO.ProjectDtos
{
    public class ProjectUpdateDto
    {
        [NotEmptyGuid]
        public Guid ProjectId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Budget must not be negative.")]
        public int Budget { get; set; }

        [Required]
        public ProjectStatus Status { get; set; }

        [FutureOrNullDate]
        public DateTime? Deadline { get; set; }
    }
}