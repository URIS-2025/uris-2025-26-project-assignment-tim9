using System.ComponentModel.DataAnnotations;
using ProjectService.Validation;

namespace ProjectService.Models.DTO.MilestoneDtos
{
    public class MilestoneCreationDto
    {
        [NotEmptyGuid]
        public Guid ProjectId { get; set; }

        [Required]
        [FutureOrNullDate]
        public DateTime ExpectedDate { get; set; }
    }
}