using System.ComponentModel.DataAnnotations;
using SprintService.Models.DTO;
using SprintService.Models.Enums;

namespace SprintService.Tests
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

        private static SprintCreationDTO ValidCreationDto() => new()
        {
            Name = "Sprint Alpha",
            Status = SprintStatus.NotStarted,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 15)
        };

        [Fact]
        public void CreationDto_WithValidData_HasNoErrors()
        {
            var results = Validate(ValidCreationDto());

            Assert.Empty(results);
        }

        [Theory]
        [InlineData("")]
        [InlineData("ab")] // below the 3-char minimum
        public void CreationDto_WithInvalidName_IsRejected(string name)
        {
            var dto = ValidCreationDto();
            dto.Name = name;

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(SprintCreationDTO.Name)));
        }

        [Fact]
        public void CreationDto_WithNameOverMaxLength_IsRejected()
        {
            var dto = ValidCreationDto();
            dto.Name = new string('x', 101);

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(SprintCreationDTO.Name)));
        }

        [Fact]
        public void CreationDto_WithEndDateBeforeStartDate_IsRejected()
        {
            var dto = ValidCreationDto();
            dto.StartDate = new DateTime(2026, 1, 15);
            dto.EndDate = new DateTime(2026, 1, 1);

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(SprintCreationDTO.EndDate)));
        }

        [Fact]
        public void CreationDto_WithEndDateEqualToStartDate_IsRejected()
        {
            var dto = ValidCreationDto();
            var same = new DateTime(2026, 1, 1);
            dto.StartDate = same;
            dto.EndDate = same;

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(SprintCreationDTO.EndDate)));
        }

        [Fact]
        public void CreationDto_WithEndDateAfterStartDate_IsAccepted()
        {
            var dto = ValidCreationDto();
            dto.StartDate = new DateTime(2026, 1, 1);
            dto.EndDate = new DateTime(2026, 1, 2);

            var results = Validate(dto);

            Assert.Empty(results);
        }

        [Fact]
        public void UpdateDto_WithValidData_HasNoErrors()
        {
            var dto = new SprintUpdateDTO
            {
                ProjectId = Guid.NewGuid(),
                Name = "Sprint Beta",
                Status = SprintStatus.Active,
                StartDate = new DateTime(2026, 2, 1),
                EndDate = new DateTime(2026, 2, 15)
            };

            var results = Validate(dto);

            Assert.Empty(results);
        }

        [Fact]
        public void UpdateDto_WithEndDateBeforeStartDate_IsRejected()
        {
            var dto = new SprintUpdateDTO
            {
                ProjectId = Guid.NewGuid(),
                Name = "Sprint Beta",
                Status = SprintStatus.Active,
                StartDate = new DateTime(2026, 2, 15),
                EndDate = new DateTime(2026, 2, 1)
            };

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(SprintUpdateDTO.EndDate)));
        }

        [Fact]
        public void UpdateDto_WithMissingName_IsRejected()
        {
            var dto = new SprintUpdateDTO
            {
                ProjectId = Guid.NewGuid(),
                Name = "",
                Status = SprintStatus.Active,
                StartDate = new DateTime(2026, 2, 1),
                EndDate = new DateTime(2026, 2, 15)
            };

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(SprintUpdateDTO.Name)));
        }
    }
}
