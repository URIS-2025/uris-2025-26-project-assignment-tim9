using System.ComponentModel.DataAnnotations;
using AttachmentService.Models;

namespace AttachmentService.Validation
{
    public class AllowedContentTypesAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is not string contentType || string.IsNullOrWhiteSpace(contentType))
            {
                return false;
            }

            return AttachmentConstraints.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} is not an allowed content type. Allowed types: {string.Join(", ", AttachmentConstraints.AllowedContentTypes)}.";
        }
    }
}
