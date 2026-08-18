using System.ComponentModel.DataAnnotations;
using ProjectService.Validation;

namespace ProjectService.Models.DTO.MilestoneDtos
{
    public class MilestoneUpdateDto
    {
        [NotEmptyGuid]
        public Guid MilestoneId { get; set; }

        [NotEmptyGuid]
        public Guid ProjectId { get; set; }

        [Required]
        [FutureOrNullDate]
        public DateTime ExpectedDate { get; set; }
    }
}