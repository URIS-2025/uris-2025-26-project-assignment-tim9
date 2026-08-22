using System.ComponentModel.DataAnnotations;
using UserService.Models.DTO.UserDtos;
using UserService.Validation;
using Xunit;

namespace UserService.Tests
{
    public class ValidationTests
    {
        private static IList<ValidationResult> Validate(object instance)
        {
            var context = new ValidationContext(instance);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(instance, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void UserCreationDto_MissingUsername_FailsValidation()
        {
            var dto = new UserCreationDto
            {
                Name = "Test",
                Username = "",
                Email = "test@example.com",
                ContactInfo = "123",
                Password = "password123"
            };

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains("Username"));
        }

        [Fact]
        public void UserCreationDto_InvalidEmail_FailsValidation()
        {
            var dto = new UserCreationDto
            {
                Name = "Test",
                Username = "testuser",
                Email = "not-an-email",
                ContactInfo = "123",
                Password = "password123"
            };

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void UserCreationDto_ShortPassword_FailsValidation()
        {
            var dto = new UserCreationDto
            {
                Name = "Test",
                Username = "testuser",
                Email = "test@example.com",
                ContactInfo = "123",
                Password = "abc"
            };

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains("Password"));
        }

        [Fact]
        public void UserCreationDto_ValidData_PassesValidation()
        {
            var dto = new UserCreationDto
            {
                Name = "Test User",
                Username = "testuser",
                Email = "test@example.com",
                ContactInfo = "123",
                Password = "password123"
            };

            var results = Validate(dto);

            Assert.Empty(results);
        }

        [Fact]
        public void NotEmptyGuidAttribute_EmptyGuid_FailsValidation()
        {
            var dto = new RoleUpdateDto
            {
                UserId = Guid.Empty,
                NewRole = UserService.Models.Enums.UserRole.Admin,
                ChangedBy = Guid.NewGuid()
            };

            var results = Validate(dto);

            Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("UserId"));
        }
    }
}
