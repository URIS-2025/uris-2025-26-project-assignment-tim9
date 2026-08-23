using System.Net;
using System.Net.Http.Json;
using NotificationService.Models.DTO.NotificationDTOs;

namespace NotificationService.Tests.IntegrationTests
{
    public class NotificationControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public NotificationControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<NotificationDisplayDTO> CreateNotificationAsync(Guid userId)
        {
            var dto = new NotificationCreateDTO { UserId = userId, Message = "Poruka " + Guid.NewGuid(), Type = "Test" };
            var response = await _client.PostAsJsonAsync("notifications", dto);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<NotificationDisplayDTO>();
            return created!;
        }

        [Fact]
        public async Task Post_WithValidPayload_Returns201AndBody()
        {
            var userId = Guid.NewGuid();
            var dto = new NotificationCreateDTO { UserId = userId, Message = "Task X je otkljucan", Type = "DependencyResolved" };

            var response = await _client.PostAsJsonAsync("notifications", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<NotificationDisplayDTO>();
            Assert.Equal(userId, body!.UserId);
            Assert.Equal("Task X je otkljucan", body.Description);
            Assert.False(body.IsRead);
        }

        [Fact]
        public async Task GetAll_WithoutUserId_ReturnsBadRequest()
        {
            var response = await _client.GetAsync("notifications");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_ReturnsOnlyNotificationsForGivenUser()
        {
            var userId = Guid.NewGuid();
            await CreateNotificationAsync(userId);
            await CreateNotificationAsync(userId);
            await CreateNotificationAsync(Guid.NewGuid());

            var response = await _client.GetAsync($"notifications?userId={userId}");

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<NotificationDisplayDTO>>();
            Assert.Equal(2, result!.Count);
            Assert.All(result, n => Assert.Equal(userId, n.UserId));
        }

        [Fact]
        public async Task GetById_WhenNotFound_Returns404()
        {
            var response = await _client.GetAsync($"notifications/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Put_MarksNotificationAsRead()
        {
            var created = await CreateNotificationAsync(Guid.NewGuid());

            var response = await _client.PutAsync($"notifications/{created.Id}", null);

            response.EnsureSuccessStatusCode();
            var updated = await response.Content.ReadFromJsonAsync<NotificationDisplayDTO>();
            Assert.True(updated!.IsRead);
        }

        [Fact]
        public async Task Put_WhenNotFound_Returns404()
        {
            var response = await _client.PutAsync($"notifications/{Guid.NewGuid()}", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
