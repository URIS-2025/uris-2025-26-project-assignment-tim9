using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ProjectService.Models.DTO.RequirementsDtos;
using Xunit;

namespace ProjectService.Tests.Integration
{
    public class RequirementsControllerTests : IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public RequirementsControllerTests()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        private static RequirementsCreationDto ValidCreationDto(Guid? projectId = null) => new RequirementsCreationDto
        {
            ProjectId = projectId ?? Guid.NewGuid(),
            Description = "The system must support user authentication."
        };

        [Fact]
        public async Task GetRequirements_WhenNoneExist_ReturnsNoContent()
        {
            // Act
            var response = await _client.GetAsync("/api/requirements");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task GetRequirements_WhenExist_ReturnsOkWithRequirements()
        {
            // Arrange
            await _client.PostAsJsonAsync("/api/requirements", ValidCreationDto());

            // Act
            var response = await _client.GetAsync("/api/requirements");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var requirements = await response.Content.ReadFromJsonAsync<RequirementsDto[]>();
            Assert.NotNull(requirements);
            Assert.Single(requirements!);
        }

        [Fact]
        public async Task GetRequirementsByProjectId_ReturnsOnlyMatchingRequirements()
        {
            // Arrange
            var projectAId = Guid.NewGuid();
            var projectBId = Guid.NewGuid();
            await _client.PostAsJsonAsync("/api/requirements", ValidCreationDto(projectAId));
            await _client.PostAsJsonAsync("/api/requirements", ValidCreationDto(projectAId));
            await _client.PostAsJsonAsync("/api/requirements", ValidCreationDto(projectBId));

            // Act
            var response = await _client.GetAsync($"/api/requirements/project/{projectAId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var requirements = await response.Content.ReadFromJsonAsync<RequirementsDto[]>();
            Assert.NotNull(requirements);
            Assert.Equal(2, requirements!.Length);
            Assert.All(requirements, r => Assert.Equal(projectAId, r.ProjectId));
        }

        [Fact]
        public async Task GetRequirementById_WhenExists_ReturnsOkWithRequirement()
        {
            // Arrange
            var createResponse = await _client.PostAsJsonAsync("/api/requirements", ValidCreationDto());
            var created = await createResponse.Content.ReadFromJsonAsync<RequirementsConfirmationDto>();

            // Act
            var response = await _client.GetAsync($"/api/requirements/{created!.RequirementId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var requirement = await response.Content.ReadFromJsonAsync<RequirementsDto>();
            Assert.NotNull(requirement);
            Assert.Equal(created.RequirementId, requirement!.RequirementId);
        }

        [Fact]
        public async Task GetRequirementById_WhenDoesNotExist_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/requirements/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateRequirement_WithValidData_ReturnsCreatedWithRequirement()
        {
            // Arrange
            var dto = ValidCreationDto();

            // Act
            var response = await _client.PostAsJsonAsync("/api/requirements", dto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<RequirementsConfirmationDto>();
            Assert.NotNull(created);
            Assert.NotEqual(Guid.Empty, created!.RequirementId);
            Assert.Equal(dto.Description, created.Description);
        }

        [Fact]
        public async Task CreateRequirement_WithEmptyProjectId_ReturnsBadRequest()
        {
            // Arrange
            var dto = ValidCreationDto();
            dto.ProjectId = Guid.Empty; // [NotEmptyGuid]

            // Act
            var response = await _client.PostAsJsonAsync("/api/requirements", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateRequirement_WithMissingDescription_ReturnsBadRequest()
        {
            // Arrange
            var dto = ValidCreationDto();
            dto.Description = ""; // [Required]

            // Act
            var response = await _client.PostAsJsonAsync("/api/requirements", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateRequirement_WithTooLongDescription_ReturnsBadRequest()
        {
            // Arrange
            var dto = ValidCreationDto();
            dto.Description = new string('a', 2001); // [StringLength(2000)]

            // Act
            var response = await _client.PostAsJsonAsync("/api/requirements", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateRequirement_WithValidData_ReturnsOkWithUpdatedRequirement()
        {
            // Arrange
            var createResponse = await _client.PostAsJsonAsync("/api/requirements", ValidCreationDto());
            var created = await createResponse.Content.ReadFromJsonAsync<RequirementsConfirmationDto>();

            var updateDto = new RequirementsUpdateDto
            {
                RequirementId = created!.RequirementId,
                ProjectId = Guid.NewGuid(),
                Description = "Updated description"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/requirements", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<RequirementsConfirmationDto>();
            Assert.NotNull(updated);
            Assert.Equal("Updated description", updated!.Description);
        }

        [Fact]
        public async Task UpdateRequirement_WhenDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var updateDto = new RequirementsUpdateDto
            {
                RequirementId = Guid.NewGuid(), // ne postoji
                ProjectId = Guid.NewGuid(),
                Description = "Doesn't matter"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/requirements", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateRequirement_WithEmptyRequirementId_ReturnsBadRequest()
        {
            // Arrange
            var updateDto = new RequirementsUpdateDto
            {
                RequirementId = Guid.Empty, // [NotEmptyGuid]
                ProjectId = Guid.NewGuid(),
                Description = "Doesn't matter"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/requirements", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteRequirement_WhenExists_ReturnsNoContent()
        {
            // Arrange
            var createResponse = await _client.PostAsJsonAsync("/api/requirements", ValidCreationDto());
            var created = await createResponse.Content.ReadFromJsonAsync<RequirementsConfirmationDto>();

            // Act
            var response = await _client.DeleteAsync($"/api/requirements/{created!.RequirementId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getResponse = await _client.GetAsync($"/api/requirements/{created.RequirementId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteRequirement_WhenDoesNotExist_ReturnsNotFound()
        {
            // Act
            var response = await _client.DeleteAsync($"/api/requirements/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
