using System.Net;
using System.Net.Http.Json;
using WorkPackageService.Models.DTO.WorkPackageDTOs;
using WorkPackageService.Models.Enums;

namespace WorkPackageService.Tests.IntegrationTests
{
    public class WorkPackageControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public WorkPackageControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Post_WithValidBody_Returns201WithGeneratedId()
        {
            // Arrange
            var dto = new WorkPackageCreateDTO
            {
                ProjectId = Guid.NewGuid(),
                Name = "Sprint 1",
                Status = WorkPackageStatus.Planned
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/workpackage", dto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<WorkPackageDisplayDTO>();
            Assert.NotNull(created);
            Assert.NotEqual(Guid.Empty, created!.WorkPackageId);
        }

        [Fact]
        public async Task Post_WithEmptyName_Returns400()
        {
            // Arrange
            var dto = new WorkPackageCreateDTO
            {
                ProjectId = Guid.NewGuid(),
                Name = "",
                Status = WorkPackageStatus.Planned
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/workpackage", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_ById_WhenExists_Returns200()
        {
            // Arrange
            var createDto = new WorkPackageCreateDTO { ProjectId = Guid.NewGuid(), Name = "WP", Status = WorkPackageStatus.Planned };
            var createResponse = await _client.PostAsJsonAsync("api/workpackage", createDto);
            var created = await createResponse.Content.ReadFromJsonAsync<WorkPackageDisplayDTO>();

            // Act
            var response = await _client.GetAsync($"api/workpackage/{created!.WorkPackageId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Get_ById_WhenNotExists_Returns404()
        {
            // Act
            var response = await _client.GetAsync($"api/workpackage/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Put_UpdatesEntity_ReflectedOnNextGet()
        {
            // Arrange
            var createDto = new WorkPackageCreateDTO { ProjectId = Guid.NewGuid(), Name = "Original", Status = WorkPackageStatus.Planned };
            var createResponse = await _client.PostAsJsonAsync("api/workpackage", createDto);
            var created = await createResponse.Content.ReadFromJsonAsync<WorkPackageDisplayDTO>();

            var updateDto = new WorkPackageUpdateDTO { Id = created!.WorkPackageId, Name = "Renamed" };

            // Act
            var putResponse = await _client.PutAsJsonAsync("api/workpackage", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

            var getResponse = await _client.GetAsync($"api/workpackage/{created.WorkPackageId}");
            var fetched = await getResponse.Content.ReadFromJsonAsync<WorkPackageDisplayDTO>();
            Assert.Equal("Renamed", fetched!.Name);
        }

        [Fact]
        public async Task Delete_RemovesEntity_SubsequentGetReturns404()
        {
            // Arrange
            var createDto = new WorkPackageCreateDTO { ProjectId = Guid.NewGuid(), Name = "ToDelete", Status = WorkPackageStatus.Planned };
            var createResponse = await _client.PostAsJsonAsync("api/workpackage", createDto);
            var created = await createResponse.Content.ReadFromJsonAsync<WorkPackageDisplayDTO>();

            // Act
            var deleteResponse = await _client.DeleteAsync($"api/workpackage/{created!.WorkPackageId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getResponse = await _client.GetAsync($"api/workpackage/{created.WorkPackageId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}
