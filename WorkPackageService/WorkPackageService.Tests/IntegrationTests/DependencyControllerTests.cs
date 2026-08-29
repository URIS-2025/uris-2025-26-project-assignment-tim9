using System.Net;
using System.Net.Http.Json;
using WorkPackageService.Models.DTO.DependencyDTOs;
using WorkPackageService.Models.DTO.TaskDTOs;
using WorkPackageService.Models.DTO.WorkPackageDTOs;
using WorkPackageService.Models.Enums;
using TaskPriority = WorkPackageService.Models.Enums.TaskPriority;
using TaskStatus = WorkPackageService.Models.Enums.TaskStatus;

namespace WorkPackageService.Tests.IntegrationTests
{
    public class DependencyControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public DependencyControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", factory.GenerateJwtToken(role: "ProjectManager"));
        }

        private async Task<Guid> CreateTaskAsync()
        {
            var wpDto = new WorkPackageCreateDTO { ProjectId = Guid.NewGuid(), Name = "WP", Status = WorkPackageStatus.Planned };
            var wpResponse = await _client.PostAsJsonAsync("api/workpackage", wpDto);
            wpResponse.EnsureSuccessStatusCode();
            var wp = await wpResponse.Content.ReadFromJsonAsync<WorkPackageDisplayDTO>();

            var taskDto = new TaskCreateDTO
            {
                WorkPackageId = wp!.WorkPackageId,
                Title = "Task " + Guid.NewGuid(),
                Status = TaskStatus.ToDo,
                Priority = TaskPriority.Medium
            };
            var taskResponse = await _client.PostAsJsonAsync("api/task", taskDto);
            taskResponse.EnsureSuccessStatusCode();
            var task = await taskResponse.Content.ReadFromJsonAsync<TaskDisplayDTO>();
            return task!.TaskId;
        }

        [Fact]
        public async Task Post_WhenTaskIdEqualsBlockerTaskId_Returns400()
        {
            // Arrange
            var taskId = await CreateTaskAsync();
            var dto = new DependencyCreateDTO { TaskId = taskId, BlockerTaskId = taskId };

            // Act
            var response = await _client.PostAsJsonAsync("api/dependency", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_WithValidDependency_Returns201()
        {
            // Arrange
            var taskId = await CreateTaskAsync();
            var blockerTaskId = await CreateTaskAsync();
            var dto = new DependencyCreateDTO { TaskId = taskId, BlockerTaskId = blockerTaskId };

            // Act
            var response = await _client.PostAsJsonAsync("api/dependency", dto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
    }
}
