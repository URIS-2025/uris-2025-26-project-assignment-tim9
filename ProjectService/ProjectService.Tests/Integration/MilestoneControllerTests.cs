using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ProjectService.Models.DTO.MilestoneDtos;
using Xunit;

namespace ProjectService.Tests.Integration
{
    public class MilestoneControllerTests : IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public MilestoneControllerTests()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        private static MilestoneCreationDto ValidCreationDto(Guid? projectId = null) => new MilestoneCreationDto
        {
            ProjectId = projectId ?? Guid.NewGuid(),
            ExpectedDate = DateTime.Now.AddMonths(1)
        };

        [Fact]
        public async Task GetMilestones_WhenNoneExist_ReturnsNoContent()
        {
            // Act
            var response = await _client.GetAsync("/api/milestone");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task GetMilestones_WhenExist_ReturnsOkWithMilestones()
        {
            // Arrange
            await _client.PostAsJsonAsync("/api/milestone", ValidCreationDto());

            // Act
            var response = await _client.GetAsync("/api/milestone");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var milestones = await response.Content.ReadFromJsonAsync<MilestoneDto[]>();
            Assert.NotNull(milestones);
            Assert.Single(milestones!);
        }

        [Fact]
        public async Task GetMilestonesByProjectId_ReturnsOnlyMatchingMilestones()
        {
            // Arrange
            var projectAId = Guid.NewGuid();
            var projectBId = Guid.NewGuid();
            await _client.PostAsJsonAsync("/api/milestone", ValidCreationDto(projectAId));
            await _client.PostAsJsonAsync("/api/milestone", ValidCreationDto(projectAId));
            await _client.PostAsJsonAsync("/api/milestone", ValidCreationDto(projectBId));

            // Act
            var response = await _client.GetAsync($"/api/milestone/project/{projectAId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var milestones = await response.Content.ReadFromJsonAsync<MilestoneDto[]>();
            Assert.NotNull(milestones);
            Assert.Equal(2, milestones!.Length);
            Assert.All(milestones, m => Assert.Equal(projectAId, m.ProjectId));
        }

        [Fact]
        public async Task GetMilestoneById_WhenExists_ReturnsOkWithMilestone()
        {
            // Arrange
            var createResponse = await _client.PostAsJsonAsync("/api/milestone", ValidCreationDto());
            var created = await createResponse.Content.ReadFromJsonAsync<MilestoneConfirmationDto>();

            // Act
            var response = await _client.GetAsync($"/api/milestone/{created!.MilestoneId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var milestone = await response.Content.ReadFromJsonAsync<MilestoneDto>();
            Assert.NotNull(milestone);
            Assert.Equal(created.MilestoneId, milestone!.MilestoneId);
        }

        [Fact]
        public async Task GetMilestoneById_WhenDoesNotExist_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/milestone/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateMilestone_WithValidData_ReturnsCreatedWithMilestone()
        {
            // Arrange
            var dto = ValidCreationDto();

            // Act
            var response = await _client.PostAsJsonAsync("/api/milestone", dto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<MilestoneConfirmationDto>();
            Assert.NotNull(created);
            Assert.NotEqual(Guid.Empty, created!.MilestoneId);
            Assert.Equal(dto.ProjectId, created.ProjectId);
        }

        [Fact]
        public async Task CreateMilestone_WithEmptyProjectId_ReturnsBadRequest()
        {
            // Arrange
            var dto = ValidCreationDto();
            dto.ProjectId = Guid.Empty; // [NotEmptyGuid]

            // Act
            var response = await _client.PostAsJsonAsync("/api/milestone", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateMilestone_WithPastExpectedDate_ReturnsBadRequest()
        {
            // Arrange
            var dto = ValidCreationDto();
            dto.ExpectedDate = DateTime.Now.AddDays(-1); // [FutureOrNullDate]

            // Act
            var response = await _client.PostAsJsonAsync("/api/milestone", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateMilestone_WithValidData_ReturnsOkWithUpdatedMilestone()
        {
            // Arrange
            var createResponse = await _client.PostAsJsonAsync("/api/milestone", ValidCreationDto());
            var created = await createResponse.Content.ReadFromJsonAsync<MilestoneConfirmationDto>();

            var newProjectId = Guid.NewGuid();
            var newDate = DateTime.Now.AddMonths(3);
            var updateDto = new MilestoneUpdateDto
            {
                MilestoneId = created!.MilestoneId,
                ProjectId = newProjectId,
                ExpectedDate = newDate
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/milestone", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<MilestoneConfirmationDto>();
            Assert.NotNull(updated);
            Assert.Equal(newProjectId, updated!.ProjectId);
        }

        [Fact]
        public async Task UpdateMilestone_WhenDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var updateDto = new MilestoneUpdateDto
            {
                MilestoneId = Guid.NewGuid(), // ne postoji
                ProjectId = Guid.NewGuid(),
                ExpectedDate = DateTime.Now.AddMonths(1)
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/milestone", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateMilestone_WithEmptyMilestoneId_ReturnsBadRequest()
        {
            // Arrange
            var updateDto = new MilestoneUpdateDto
            {
                MilestoneId = Guid.Empty, // [NotEmptyGuid]
                ProjectId = Guid.NewGuid(),
                ExpectedDate = DateTime.Now.AddMonths(1)
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/milestone", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteMilestone_WhenExists_ReturnsNoContent()
        {
            // Arrange
            var createResponse = await _client.PostAsJsonAsync("/api/milestone", ValidCreationDto());
            var created = await createResponse.Content.ReadFromJsonAsync<MilestoneConfirmationDto>();

            // Act
            var response = await _client.DeleteAsync($"/api/milestone/{created!.MilestoneId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getResponse = await _client.GetAsync($"/api/milestone/{created.MilestoneId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteMilestone_WhenDoesNotExist_ReturnsNotFound()
        {
            // Act
            var response = await _client.DeleteAsync($"/api/milestone/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
