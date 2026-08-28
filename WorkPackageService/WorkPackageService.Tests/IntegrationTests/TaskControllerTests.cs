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
    public class TaskControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public TaskControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", factory.GenerateJwtToken(role: "ProjectManager"));
        }

        private async Task<Guid> CreateWorkPackageAsync()
        {
            var dto = new WorkPackageCreateDTO { ProjectId = Guid.NewGuid(), Name = "WP", Status = WorkPackageStatus.Planned };
            var response = await _client.PostAsJsonAsync("api/workpackage", dto);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<WorkPackageDisplayDTO>();
            return created!.WorkPackageId;
        }

        private async Task<TaskDisplayDTO> CreateTaskAsync(Guid workPackageId, Guid? assigneeId = null, Guid? parentTaskId = null)
        {
            var dto = new TaskCreateDTO
            {
                WorkPackageId = workPackageId,
                ParentTaskId = parentTaskId,
                Title = "Task " + Guid.NewGuid(),
                Status = TaskStatus.ToDo,
                Priority = TaskPriority.Medium,
                AssigneeId = assigneeId
            };
            var response = await _client.PostAsJsonAsync("api/task", dto);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<TaskDisplayDTO>();
            return created!;
        }

        [Fact]
        public async Task Post_CreatesTopLevelTask_Returns201()
        {
            // Arrange
            var workPackageId = await CreateWorkPackageAsync();
            var dto = new TaskCreateDTO
            {
                WorkPackageId = workPackageId,
                Title = "Top level task",
                Status = TaskStatus.ToDo,
                Priority = TaskPriority.Low
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/task", dto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Post_WithParentTaskId_CreatesSubTask_VisibleViaParentEndpoint()
        {
            // Arrange
            var workPackageId = await CreateWorkPackageAsync();
            var parentTask = await CreateTaskAsync(workPackageId);

            var subTaskDto = new TaskCreateDTO
            {
                WorkPackageId = workPackageId,
                ParentTaskId = parentTask.TaskId,
                Title = "Sub task",
                Status = TaskStatus.ToDo,
                Priority = TaskPriority.Low
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/task", subTaskDto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<TaskDisplayDTO>();

            var subTasksResponse = await _client.GetAsync($"api/task/parent/{parentTask.TaskId}");
            Assert.Equal(HttpStatusCode.OK, subTasksResponse.StatusCode);
            var subTasks = await subTasksResponse.Content.ReadFromJsonAsync<List<TaskDisplayDTO>>();
            Assert.Contains(subTasks!, t => t.TaskId == created!.TaskId);
        }

        [Fact]
        public async Task PatchStatus_WithCorrectAssignee_Returns200()
        {
            // Arrange
            var workPackageId = await CreateWorkPackageAsync();
            var assigneeId = Guid.NewGuid();
            var task = await CreateTaskAsync(workPackageId, assigneeId);
            var statusDto = new TaskStatusUpdateRequestDTO { NewStatus = TaskStatus.InProgress };

            // Act
            var response = await _client.PatchAsJsonAsync($"api/task/{task.TaskId}/status?callerId={assigneeId}", statusDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PatchStatus_WithWrongCaller_Returns403()
        {
            // Arrange
            var workPackageId = await CreateWorkPackageAsync();
            var assigneeId = Guid.NewGuid();
            var wrongCallerId = Guid.NewGuid();
            var task = await CreateTaskAsync(workPackageId, assigneeId);
            var statusDto = new TaskStatusUpdateRequestDTO { NewStatus = TaskStatus.InProgress };

            // Act
            var response = await _client.PatchAsJsonAsync($"api/task/{task.TaskId}/status?callerId={wrongCallerId}", statusDto);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task PatchReassign_UpdatesAssignee_ReflectedOnNextGet()
        {
            // Arrange
            var workPackageId = await CreateWorkPackageAsync();
            var oldAssigneeId = Guid.NewGuid();
            var newAssigneeId = Guid.NewGuid();
            var task = await CreateTaskAsync(workPackageId, oldAssigneeId);
            var reassignDto = new TaskReassignRequestDTO { NewAssigneeId = newAssigneeId };

            // Act
            var response = await _client.PatchAsJsonAsync($"api/task/{task.TaskId}/reassign", reassignDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var getResponse = await _client.GetAsync($"api/task/{task.TaskId}");
            var fetched = await getResponse.Content.ReadFromJsonAsync<TaskDisplayDTO>();
            Assert.Equal(newAssigneeId, fetched!.AssigneeId);
        }

        [Fact]
        public async Task PatchMove_WhenTaskHasDependency_Returns200WithWarning()
        {
            // Arrange
            var workPackageId = await CreateWorkPackageAsync();
            var newWorkPackageId = await CreateWorkPackageAsync();
            var task = await CreateTaskAsync(workPackageId);
            var blockerTask = await CreateTaskAsync(workPackageId);

            var dependencyDto = new DependencyCreateDTO { TaskId = task.TaskId, BlockerTaskId = blockerTask.TaskId };
            var depResponse = await _client.PostAsJsonAsync("api/dependency", dependencyDto);
            depResponse.EnsureSuccessStatusCode();

            var moveDto = new TaskMoveRequestDTO { NewWorkPackageId = newWorkPackageId };

            // Act
            var response = await _client.PatchAsJsonAsync($"api/task/{task.TaskId}/move", moveDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<TaskMoveResultDTO>();
            Assert.True(result!.HasDependencyWarning);
            Assert.NotNull(result.Warning);
        }
    }
}
