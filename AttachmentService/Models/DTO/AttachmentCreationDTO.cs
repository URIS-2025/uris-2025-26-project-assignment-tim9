using System.ComponentModel.DataAnnotations;
using AttachmentService.Validation;

namespace AttachmentService.Models.DTO
{
    public class AttachmentCreationDTO : IValidatableObject
    {
        [Required(ErrorMessage = "OriginalFileName is required.")]
        [StringLength(255, MinimumLength = 1)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ContentType is required.")]
        [AllowedContentTypes]
        public string ContentType { get; set; } = string.Empty;

        [MaxFileSize]
        public long FileSize { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        public Guid? WorkPackageId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ProjectId == Guid.Empty)
            {
                yield return new ValidationResult(
                    "ProjectId is required and cannot be an empty GUID.",
                    new[] { nameof(ProjectId) });
            }

            if (WorkPackageId.HasValue && WorkPackageId.Value == Guid.Empty)
            {
                yield return new ValidationResult(
                    "WorkPackageId cannot be an empty GUID when provided - omit it instead to attach directly to the project.",
                    new[] { nameof(WorkPackageId) });
            }
        }
    }
}
