using System.ComponentModel.DataAnnotations;
using AttachmentService.Models;

namespace AttachmentService.Validation
{
     public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly long _maxSizeBytes;

        public MaxFileSizeAttribute(long maxSizeBytes = AttachmentConstraints.MaxFileSizeBytes)
        {
            _maxSizeBytes = maxSizeBytes;
        }

        public override bool IsValid(object? value)
        {
            if (value is not long fileSize)
            {
                return false;
            }

            return fileSize > 0 && fileSize <= _maxSizeBytes;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be greater than 0 and not exceed {_maxSizeBytes / (1024 * 1024)} MB.";
        }
    }
}
