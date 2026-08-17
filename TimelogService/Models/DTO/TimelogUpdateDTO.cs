using System.ComponentModel.DataAnnotations;
using TimelogService.Validation;

namespace TimelogService.Models.DTO
{
    public class TimelogUpdateDTO : IValidatableObject
    {
        public Guid? ProjectId { get; set; }
        public Guid? WorkPackageId { get; set; }

        [Range(0.01, 24, ErrorMessage = "Hours spent must be greater than 0 and no more than 24.")]
        public double? HoursSpent { get; set; }

        [NoFutureDate(ErrorMessage = "Timelog date can not be in the future time.")]
        public DateTime? Date { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ProjectId.HasValue && ProjectId.Value == Guid.Empty)
            {
                yield return new ValidationResult(
                    "ProjectId cannot be an empty GUID when provided.",
                    new[] { nameof(ProjectId) });
            }

            if (WorkPackageId.HasValue && WorkPackageId.Value == Guid.Empty)
            {
                yield return new ValidationResult(
                    "WorkPackageId cannot be an empty GUID when provided.",
                    new[] { nameof(WorkPackageId) });
            }
        }
    }
}
