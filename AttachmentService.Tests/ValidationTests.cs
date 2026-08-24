using System.ComponentModel.DataAnnotations;
using AttachmentService.Models.DTO;

namespace AttachmentService.Tests
{
    public class ValidationTests
    {
        private static IList<ValidationResult> Validate(object instance)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(instance);
            Validator.TryValidateObject(instance, context, results, validateAllProperties: true);
            return results;
        }

        private static AttachmentCreationDTO ValidCreationDto() => new()
        {
            OriginalFileName = "report.pdf",
            ContentType = "application/pdf",
            FileSize = 1024,
            ProjectId = Guid.NewGuid()
        };

        [Fact]
        public void CreationDto_WithValidData_HasNoErrors()
        {
            var results = Validate(ValidCreationDto());

            Assert.Empty(results);
        }

        [Fact]
        public void CreationDto_WithValidTaskId_HasNoErrors()
        {
            var dto = ValidCreationDto();
            dto.TaskId = Guid.NewGuid();

            var results = Validate(dto);

            Assert.Empty(results);
        }

        [Fact]
        public void CreationDto_WithMissingOriginalFileName_IsRejected()
        {
            var dto = ValidCreationDto();
            dto.OriginalFileName = "";

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(AttachmentCreationDTO.OriginalFileName)));
        }

        [Fact]
        public void CreationDto_WithDisallowedContentType_IsRejected()
        {
            var dto = ValidCreationDto();
            dto.ContentType = "application/x-msdownload";

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(AttachmentCreationDTO.ContentType)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(26L * 1024 * 1024)] // one byte over the 25MB limit
        public void CreationDto_WithInvalidFileSize_IsRejected(long fileSize)
        {
            var dto = ValidCreationDto();
            dto.FileSize = fileSize;

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(AttachmentCreationDTO.FileSize)));
        }

        [Fact]
        public void CreationDto_AtExactlyMaxFileSize_IsAccepted()
        {
            var dto = ValidCreationDto();
            dto.FileSize = 25L * 1024 * 1024;

            var results = Validate(dto);

            Assert.Empty(results);
        }

        [Fact]
        public void CreationDto_WithEmptyProjectId_IsRejected()
        {
            var dto = ValidCreationDto();
            dto.ProjectId = Guid.Empty;

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(AttachmentCreationDTO.ProjectId)));
        }

        [Fact]
        public void CreationDto_WithEmptyTaskId_IsRejected()
        {
            var dto = ValidCreationDto();
            dto.TaskId = Guid.Empty;

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(AttachmentCreationDTO.TaskId)));
        }

        [Fact]
        public void ConfirmationDto_WithNullChecksum_HasNoErrors()
        {
            var dto = new AttachmentConfirmationDTO { AttachmentId = Guid.NewGuid(), Checksum = null };

            var results = Validate(dto);

            Assert.Empty(results);
        }

        [Fact]
        public void ConfirmationDto_WithValidSha256Checksum_HasNoErrors()
        {
            var dto = new AttachmentConfirmationDTO
            {
                AttachmentId = Guid.NewGuid(),
                Checksum = new string('a', 64)
            };

            var results = Validate(dto);

            Assert.Empty(results);
        }

        [Theory]
        [InlineData("not-a-checksum")]
        [InlineData("abc123")] // too short
        public void ConfirmationDto_WithMalformedChecksum_IsRejected(string checksum)
        {
            var dto = new AttachmentConfirmationDTO { AttachmentId = Guid.NewGuid(), Checksum = checksum };

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(AttachmentConfirmationDTO.Checksum)));
        }

        [Fact]
        public void UpdateDto_WithBothFieldsNull_HasNoErrors()
        {
            var dto = new AttachmentUpdateDTO();

            var results = Validate(dto);

            Assert.Empty(results);
        }

        [Fact]
        public void UpdateDto_WithDescriptionOverLimit_IsRejected()
        {
            var dto = new AttachmentUpdateDTO { Description = new string('x', 1001) };

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(AttachmentUpdateDTO.Description)));
        }
    }
}
