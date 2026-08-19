using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ProjectService.Models.DTO.ProjectDtos;
using ProjectService.Models.DTO.ProjectMemberDtos;
using ProjectService.Models.Enums;
using Xunit;

namespace ProjectService.Tests.Integration
{
    public class ProjectControllerTests : IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public ProjectControllerTests()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _factory.GenerateJwtToken(role: "ProjectManager"));
        }

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        private void UseRole(string role)
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _factory.GenerateJwtToken(role: role));
        }

        private static ProjectCreationDto ValidCreationDto() => new ProjectCreationDto
        {
            Name = "Integration Test Project",
            Budget = 1000,
            Status = ProjectStatus.Planned,
            Deadline = DateTime.Now.AddMonths(1)
        };

        [Fact]
        public async Task GetProjects_WhenNoProjectsExist_ReturnsNoContent()
        {
            // Act
            var response = await _client.GetAsync("/api/project");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task GetProjects_WhenProjectsExist_ReturnsOkWithProjects()
        {
            // Arrange
            await _client.PostAsJsonAsync("/api/project", ValidCreationDto());

            // Act
            var response = await _client.GetAsync("/api/project");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var projects = await response.Content.ReadFromJsonAsync<ProjectDto[]>();
            Assert.NotNull(projects);
            Assert.Single(projects!);
        }

        [Fact]
        public async Task GetProjectsByUserId_ReturnsOnlyProjectsForGivenUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            var projectAResponse = await _client.PostAsJsonAsync("/api/project", ValidCreationDto());
            var projectA = await projectAResponse.Content.ReadFromJsonAsync<ProjectConfirmationDto>();

            var projectBResponse = await _client.PostAsJsonAsync("/api/project", ValidCreationDto());
            var projectB = await projectBResponse.Content.ReadFromJsonAsync<ProjectConfirmationDto>();

            UseRole("Admin"); // ProjectMember POST zahteva Admin
            await _client.PostAsJsonAsync("/api/projectmember", new ProjectMemberCreationDto
            {
                ProjectId = projectA!.ProjectId,
                UserId = userId
            });
            await _client.PostAsJsonAsync("/api/projectmember", new ProjectMemberCreationDto
            {
                ProjectId = projectB!.ProjectId,
                UserId = otherUserId
            });

            // Act
            var response = await _client.GetAsync($"/api/project/user/{userId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var projects = await response.Content.ReadFromJsonAsync<ProjectDto[]>();
            Assert.NotNull(projects);
            Assert.Single(projects!);
            Assert.Equal(projectA.ProjectId, projects![0].ProjectId);
        }

        [Fact]
        public async Task GetProjectById_WhenProjectExists_ReturnsOkWithProject()
        {
            // Arrange
            var createResponse = await _client.PostAsJsonAsync("/api/project", ValidCreationDto());
            var created = await createResponse.Content.ReadFromJsonAsync<ProjectConfirmationDto>();

            // Act
            var response = await _client.GetAsync($"/api/project/{created!.ProjectId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(project);
            Assert.Equal(created.ProjectId, project!.ProjectId);
        }

        [Fact]
        public async Task GetProjectById_WhenProjectDoesNotExist_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/project/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateProject_WithValidData_ReturnsCreatedWithProject()
        {
            // Arrange
            var dto = ValidCreationDto();

            // Act
            var response = await _client.PostAsJsonAsync("/api/project", dto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<ProjectConfirmationDto>();
            Assert.NotNull(created);
            Assert.NotEqual(Guid.Empty, created!.ProjectId);
            Assert.Equal(dto.Name, created.Name);
        }

        [Fact]
        public async Task CreateProject_WithMissingName_ReturnsBadRequest()
        {
            // Arrange
            var dto = ValidCreationDto();
            dto.Name = ""; // [Required] ne dozvoljava prazan string

            // Act
            var response = await _client.PostAsJsonAsync("/api/project", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateProject_WithNegativeBudget_ReturnsBadRequest()
        {
            // Arrange
            var dto = ValidCreationDto();
            dto.Budget = -100; // [Range(0, int.MaxValue)]

            // Act
            var response = await _client.PostAsJsonAsync("/api/project", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateProject_WithMissingStatus_ReturnsBadRequest()
        {
            // Arrange
            var dto = ValidCreationDto();
            dto.Status = null; // [Required] na ProjectStatus?

            // Act
            var response = await _client.PostAsJsonAsync("/api/project", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateProject_WithPastDeadline_ReturnsBadRequest()
        {
            // Arrange
            var dto = ValidCreationDto();
            dto.Deadline = DateTime.Now.AddDays(-1); // [FutureOrNullDate]

            // Act
            var response = await _client.PostAsJsonAsync("/api/project", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateProject_WithValidData_ReturnsOkWithUpdatedProject()
        {
            // Arrange
            var createResponse = await _client.PostAsJsonAsync("/api/project", ValidCreationDto());
            var created = await createResponse.Content.ReadFromJsonAsync<ProjectConfirmationDto>();

            var updateDto = new ProjectUpdateDto
            {
                ProjectId = created!.ProjectId,
                Name = "Updated Name",
                Budget = 9999,
                Status = ProjectStatus.Active,
                Deadline = DateTime.Now.AddMonths(2)
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/project", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<ProjectConfirmationDto>();
            Assert.NotNull(updated);
            Assert.Equal("Updated Name", updated!.Name);
            Assert.Equal(9999, updated.Budget);
        }

        [Fact]
        public async Task UpdateProject_WhenProjectDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var updateDto = new ProjectUpdateDto
            {
                ProjectId = Guid.NewGuid(), // ne postoji
                Name = "Doesn't matter",
                Budget = 100,
                Status = ProjectStatus.Planned,
                Deadline = null
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/project", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateProject_WithEmptyProjectId_ReturnsBadRequest()
        {
            // Arrange
            var updateDto = new ProjectUpdateDto
            {
                ProjectId = Guid.Empty, // [NotEmptyGuid]
                Name = "Doesn't matter",
                Budget = 100,
                Status = ProjectStatus.Planned,
                Deadline = null
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/project", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteProject_WhenProjectExists_ReturnsNoContent()
        {
            // Arrange
            var createResponse = await _client.PostAsJsonAsync("/api/project", ValidCreationDto());
            var created = await createResponse.Content.ReadFromJsonAsync<ProjectConfirmationDto>();

            // Act
            UseRole("Admin"); // DELETE zahteva iskljucivo Admin
            var response = await _client.DeleteAsync($"/api/project/{created!.ProjectId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getResponse = await _client.GetAsync($"/api/project/{created.ProjectId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteProject_WhenProjectDoesNotExist_ReturnsNotFound()
        {
            // Act
            UseRole("Admin"); // DELETE zahteva iskljucivo Admin
            var response = await _client.DeleteAsync($"/api/project/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
