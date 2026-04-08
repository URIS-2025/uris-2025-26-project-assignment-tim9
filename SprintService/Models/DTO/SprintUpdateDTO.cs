using SprintService.Models.Enums;
using SprintService.Validation;
using System.ComponentModel.DataAnnotations;

namespace SprintService.Models.DTO
{
    public class SprintUpdateDTO
    {
        [Required(ErrorMessage = "Sprint ID is required.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Project ID is required.")]
        public Guid ProjectId { get; set; }

        [Required(ErrorMessage = "Sprint name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must have between 3 and 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public SprintStatus Status { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        [DateGreaterThan("StartDate", ErrorMessage = "End date must be after start date")]
        public DateTime EndDate { get; set; }
    }
}
