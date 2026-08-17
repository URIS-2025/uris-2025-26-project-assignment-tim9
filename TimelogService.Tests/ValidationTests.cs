using System.ComponentModel.DataAnnotations;
using TimelogService.Models.DTO;

namespace TimelogService.Tests
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

        private static TimelogCreationDTO ValidCreationDto() => new()
        {
            ProjectId = Guid.NewGuid(),
            WorkPackageId = Guid.NewGuid(),
            HoursSpent = 4,
            Date = DateTime.Now.AddDays(-1)
        };

        [Fact]
        public void CreationDto_WithValidData_HasNoErrors()
        {
            var results = Validate(ValidCreationDto());

            Assert.Empty(results);
        }

        [Fact]
        public void CreationDto_WithEmptyProjectId_IsRejected()
        {
            var dto = ValidCreationDto();
            dto.ProjectId = Guid.Empty;

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(TimelogCreationDTO.ProjectId)));
        }

        [Fact]
        public void CreationDto_WithEmptyWorkPackageId_IsRejected()
        {
            var dto = ValidCreationDto();
            dto.WorkPackageId = Guid.Empty;

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(TimelogCreationDTO.WorkPackageId)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(24.01)]
        public void CreationDto_WithInvalidHoursSpent_IsRejected(double hours)
        {
            var dto = ValidCreationDto();
            dto.HoursSpent = hours;

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(TimelogCreationDTO.HoursSpent)));
        }

        [Fact]
        public void CreationDto_WithHoursSpentAtBounds_IsAccepted()
        {
            var dto = ValidCreationDto();
            dto.HoursSpent = 24;

            var results = Validate(dto);

            Assert.Empty(results);
        }

        [Fact]
        public void CreationDto_WithFutureDate_IsRejected()
        {
            var dto = ValidCreationDto();
            dto.Date = DateTime.Now.AddDays(5);

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(TimelogCreationDTO.Date)));
        }

        [Fact]
        public void CreationDto_WithPastDate_IsAccepted()
        {
            var dto = ValidCreationDto();
            dto.Date = DateTime.Now.AddDays(-30);

            var results = Validate(dto);

            Assert.Empty(results);
        }

        [Fact]
        public void UpdateDto_WithAllFieldsNull_HasNoErrors()
        {
            var dto = new TimelogUpdateDTO();

            var results = Validate(dto);

            Assert.Empty(results);
        }

        [Fact]
        public void UpdateDto_WithOnlyHoursSpentSet_HasNoErrors()
        {
            var dto = new TimelogUpdateDTO { HoursSpent = 6 };

            var results = Validate(dto);

            Assert.Empty(results);
        }

        [Fact]
        public void UpdateDto_WithInvalidHoursSpent_IsRejected()
        {
            var dto = new TimelogUpdateDTO { HoursSpent = -5 };

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(TimelogUpdateDTO.HoursSpent)));
        }

        [Fact]
        public void UpdateDto_WithFutureDate_IsRejected()
        {
            var dto = new TimelogUpdateDTO { Date = DateTime.Now.AddDays(2) };

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(TimelogUpdateDTO.Date)));
        }

        [Fact]
        public void UpdateDto_WithEmptyProjectId_IsRejected()
        {
            var dto = new TimelogUpdateDTO { ProjectId = Guid.Empty };

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(TimelogUpdateDTO.ProjectId)));
        }
    }
}
