using System.Net;
using System.Net.Http.Json;
using WorkPackageService.Models.DTO.CommentDTOs;
using WorkPackageService.Models.DTO.TaskDTOs;
using WorkPackageService.Models.DTO.WorkPackageDTOs;
using WorkPackageService.Models.Enums;
using TaskPriority = WorkPackageService.Models.Enums.TaskPriority;
using TaskStatus = WorkPackageService.Models.Enums.TaskStatus;

namespace WorkPackageService.Tests.IntegrationTests
{
    public class CommentControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public CommentControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
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

        private async Task<CommentDisplayDTO> CreateCommentAsync(Guid taskId, Guid authorId)
        {
            var dto = new CommentCreateDTO { TaskId = taskId, AuthorId = authorId, Text = "Original text" };
            var response = await _client.PostAsJsonAsync("api/comment", dto);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<CommentDisplayDTO>();
            return created!;
        }

        // Napomena: implementirana ruta iz Faze 6c je PUT api/comment (Id u telu, ne u ruti) -
        // testiram stvarnu rutu, ne PUT api/comment/{id} kako je opisano u ovoj fazi.
        [Fact]
        public async Task Put_WithCorrectAuthor_Returns200()
        {
            // Arrange
            var taskId = await CreateTaskAsync();
            var authorId = Guid.NewGuid();
            var comment = await CreateCommentAsync(taskId, authorId);
            var updateDto = new CommentUpdateDTO { Id = comment.CommentId, Text = "Updated text" };

            // Act
            var response = await _client.PutAsJsonAsync($"api/comment?callerId={authorId}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Put_WithWrongCaller_Returns403()
        {
            // Arrange
            var taskId = await CreateTaskAsync();
            var authorId = Guid.NewGuid();
            var wrongCallerId = Guid.NewGuid();
            var comment = await CreateCommentAsync(taskId, authorId);
            var updateDto = new CommentUpdateDTO { Id = comment.CommentId, Text = "Hacked text" };

            // Act
            var response = await _client.PutAsJsonAsync($"api/comment?callerId={wrongCallerId}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
