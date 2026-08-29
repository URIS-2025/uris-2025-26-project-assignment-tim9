using System.ComponentModel.DataAnnotations;
using ProjectService.Validation;

namespace ProjectService.Models.DTO.MilestoneDtos
{
    public class MilestoneCreationDto
    {
        [NotEmptyGuid]
        public Guid ProjectId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [FutureOrNullDate]
        public DateTime ExpectedDate { get; set; }
    }
}
