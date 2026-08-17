using System.ComponentModel.DataAnnotations;
using TimelogService.Validation;

namespace TimelogService.Models.DTO
{
    public class TimelogCreationDTO : IValidatableObject
    {
        [Required(ErrorMessage = "Project ID is required.")]
        public Guid ProjectId { get; set; }

        [Required(ErrorMessage = "WorkPackage ID is required.")]
        public Guid WorkPackageId { get; set; }

        [Range(0.01, 24, ErrorMessage = "Hours spent must be greater than 0 and no more than 24.")]
        public double HoursSpent { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [NoFutureDate(ErrorMessage = "Timelog date can not be in the future time.")]
        public DateTime Date { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ProjectId == Guid.Empty)
            {
                yield return new ValidationResult(
                    "ProjectId is required and cannot be an empty GUID.",
                    new[] { nameof(ProjectId) });
            }

            if (WorkPackageId == Guid.Empty)
            {
                yield return new ValidationResult(
                    "WorkPackageId is required and cannot be an empty GUID.",
                    new[] { nameof(WorkPackageId) });
            }
        }
    }
}
