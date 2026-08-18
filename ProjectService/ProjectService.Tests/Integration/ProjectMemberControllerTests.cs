using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ProjectService.Models.DTO.ProjectMemberDtos;
using Xunit;

namespace ProjectService.Tests.Integration
{
    public class ProjectMemberControllerTests : IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public ProjectMemberControllerTests()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        private static ProjectMemberCreationDto ValidCreationDto(Guid? projectId = null) => new ProjectMemberCreationDto
        {
            ProjectId = projectId ?? Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        [Fact]
        public async Task GetProjectMembers_WhenNoneExist_ReturnsNoContent()
        {
            // Act
            var response = await _client.GetAsync("/api/projectmember");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task GetProjectMembers_WhenExist_ReturnsOkWithMembers()
        {
            // Arrange
            await _client.PostAsJsonAsync("/api/projectmember", ValidCreationDto());

            // Act
            var response = await _client.GetAsync("/api/projectmember");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var members = await response.Content.ReadFromJsonAsync<ProjectMemberDto[]>();
            Assert.NotNull(members);
            Assert.Single(members!);
        }

        [Fact]
        public async Task GetMembersByProjectId_ReturnsOnlyMatchingMembers()
        {
            // Arrange
            var projectAId = Guid.NewGuid();
            var projectBId = Guid.NewGuid();
            await _client.PostAsJsonAsync("/api/projectmember", ValidCreationDto(projectAId));
            await _client.PostAsJsonAsync("/api/projectmember", ValidCreationDto(projectAId));
            await _client.PostAsJsonAsync("/api/projectmember", ValidCreationDto(projectBId));

            // Act
            var response = await _client.GetAsync($"/api/projectmember/project/{projectAId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var members = await response.Content.ReadFromJsonAsync<ProjectMemberDto[]>();
            Assert.NotNull(members);
            Assert.Equal(2, members!.Length);
            Assert.All(members, m => Assert.Equal(projectAId, m.ProjectId));
        }

        [Fact]
        public async Task GetProjectMemberById_WhenExists_ReturnsOkWithMember()
        {
            // Arrange
            var createResponse = await _client.PostAsJsonAsync("/api/projectmember", ValidCreationDto());
            var created = await createResponse.Content.ReadFromJsonAsync<ProjectMemberConfirmationDto>();

            // Act
            var response = await _client.GetAsync($"/api/projectmember/{created!.ProjectMemberId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var member = await response.Content.ReadFromJsonAsync<ProjectMemberDto>();
            Assert.NotNull(member);
            Assert.Equal(created.ProjectMemberId, member!.ProjectMemberId);
        }

        [Fact]
        public async Task GetProjectMemberById_WhenDoesNotExist_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/projectmember/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateProjectMember_WithValidData_ReturnsCreatedWithMember()
        {
            // Arrange
            var dto = ValidCreationDto();

            // Act
            var response = await _client.PostAsJsonAsync("/api/projectmember", dto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<ProjectMemberConfirmationDto>();
            Assert.NotNull(created);
            Assert.NotEqual(Guid.Empty, created!.ProjectMemberId);
            Assert.Equal(dto.ProjectId, created.ProjectId);
            Assert.Equal(dto.UserId, created.UserId);
        }

        [Fact]
        public async Task CreateProjectMember_WithEmptyProjectId_ReturnsBadRequest()
        {
            // Arrange
            var dto = ValidCreationDto();
            dto.ProjectId = Guid.Empty; // [NotEmptyGuid]

            // Act
            var response = await _client.PostAsJsonAsync("/api/projectmember", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateProjectMember_WithEmptyUserId_ReturnsBadRequest()
        {
            // Arrange
            var dto = ValidCreationDto();
            dto.UserId = Guid.Empty; // [NotEmptyGuid]

            // Act
            var response = await _client.PostAsJsonAsync("/api/projectmember", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateProjectMember_WithValidData_ReturnsOkWithUpdatedMember()
        {
            // Arrange
            var createResponse = await _client.PostAsJsonAsync("/api/projectmember", ValidCreationDto());
            var created = await createResponse.Content.ReadFromJsonAsync<ProjectMemberConfirmationDto>();

            var updateDto = new ProjectMemberUpdateDto
            {
                ProjectMemberId = created!.ProjectMemberId,
                ProjectId = created.ProjectId,
                UserId = created.UserId,
                JoinedAt = created.JoinedAt,
                Status = false
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/projectmember", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<ProjectMemberConfirmationDto>();
            Assert.NotNull(updated);
            Assert.False(updated!.Status);
        }

        [Fact]
        public async Task UpdateProjectMember_WhenDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var updateDto = new ProjectMemberUpdateDto
            {
                ProjectMemberId = Guid.NewGuid(), // ne postoji
                ProjectId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                JoinedAt = DateTime.UtcNow,
                Status = true
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/projectmember", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateProjectMember_WithEmptyProjectMemberId_ReturnsBadRequest()
        {
            // Arrange
            var updateDto = new ProjectMemberUpdateDto
            {
                ProjectMemberId = Guid.Empty, // [NotEmptyGuid]
                ProjectId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                JoinedAt = DateTime.UtcNow,
                Status = true
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/projectmember", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteProjectMember_WhenExists_ReturnsNoContent()
        {
            // Arrange
            var createResponse = await _client.PostAsJsonAsync("/api/projectmember", ValidCreationDto());
            var created = await createResponse.Content.ReadFromJsonAsync<ProjectMemberConfirmationDto>();

            // Act
            var response = await _client.DeleteAsync($"/api/projectmember/{created!.ProjectMemberId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getResponse = await _client.GetAsync($"/api/projectmember/{created.ProjectMemberId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteProjectMember_WhenDoesNotExist_ReturnsNotFound()
        {
            // Act
            var response = await _client.DeleteAsync($"/api/projectmember/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
