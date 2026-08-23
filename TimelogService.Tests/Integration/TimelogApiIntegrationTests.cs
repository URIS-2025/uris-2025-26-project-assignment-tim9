using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TimelogService.Models.DTO;

namespace TimelogService.Tests.Integration
{
    /// <summary>
    /// Exercises the real HTTP pipeline (routing, model binding/validation, DI, controller,
    /// repository) against real local MySQL, plus fake HTTP stand-ins for User Service and
    /// WorkPackage Service. Requires a local MySQL reachable on localhost:3306 (root/root).
    /// </summary>
    public class TimelogApiIntegrationTests : IClassFixture<TimelogApiFixture>
    {
        private readonly TimelogApiFixture _fx;

        public TimelogApiIntegrationTests(TimelogApiFixture fixture)
        {
            _fx = fixture;
        }

        private static HttpRequestMessage CreateRequest(HttpMethod method, string url, Guid? userId = null, object? body = null)
        {
            var request = new HttpRequestMessage(method, url);
            if (userId is not null)
            {
                request.Headers.Add("X-User-Id", userId.ToString());
            }
            if (body is not null)
            {
                request.Content = JsonContent.Create(body);
            }
            return request;
        }

        [Fact]
        public async Task FullLifecycle_CreateGetUpdateDelete_WorksEndToEnd()
        {
            // 1) Create, with both fake services recognizing the ids used - proves real
            // enrichment through Controller -> Repository -> IUserService/ITaskService
            // -> HttpClient -> fake server -> JSON deserialize.
            var createRequest = CreateRequest(HttpMethod.Post, "/api/timelog", _fx.KnownUserId, new
            {
                projectId = Guid.NewGuid(),
                taskId = _fx.KnownTaskId,
                hoursSpent = 3.5,
                date = DateTime.Now.AddDays(-1)
            });
            var createResponse = await _fx.Client.SendAsync(createRequest);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            Assert.NotNull(createResponse.Headers.Location);
            var created = await createResponse.Content.ReadFromJsonAsync<TimelogConfirmationDTO>();
            Assert.NotNull(created);
            Assert.Equal(TimelogApiFixture.KnownUsername, created!.Username);
            Assert.Equal("integration.user@example.com", created.Email);
            Assert.Equal(TimelogApiFixture.KnownUserRole, created.UserRole);
            Assert.Equal(TimelogApiFixture.KnownTaskTitle, created.TaskTitle);
            Assert.Equal(TimelogApiFixture.KnownTaskStatus, created.TaskStatus);

            // 2) Get it back
            var getResponse = await _fx.Client.GetAsync($"/api/timelog/{created.Id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var fetched = await getResponse.Content.ReadFromJsonAsync<TimelogDTO>();
            Assert.Equal(3.5, fetched!.HoursSpent);
            Assert.Equal(_fx.KnownUserId, fetched.LoggedByUserId);

            // 3) Partial update - the actual bug found during manual testing was that this
            // wiped ProjectId/TaskId/Date to their zero-defaults even though only
            // hoursSpent was sent. Prove it really only changes what was sent.
            var updateRequest = CreateRequest(HttpMethod.Put, $"/api/timelog/{created.Id}", _fx.KnownUserId, new { hoursSpent = 6.0 });
            var updateResponse = await _fx.Client.SendAsync(updateRequest);
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

            var refetched = await (await _fx.Client.GetAsync($"/api/timelog/{created.Id}")).Content.ReadFromJsonAsync<TimelogDTO>();
            Assert.Equal(6.0, refetched!.HoursSpent);
            Assert.Equal(fetched.ProjectId, refetched.ProjectId);
            Assert.Equal(fetched.TaskId, refetched.TaskId);
            Assert.Equal(fetched.Date, refetched.Date);

            // 4) Delete, then confirm it's really gone
            var deleteRequest = CreateRequest(HttpMethod.Delete, $"/api/timelog/{created.Id}", _fx.KnownUserId);
            var deleteResponse = await _fx.Client.SendAsync(deleteRequest);
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getAfterDelete = await _fx.Client.GetAsync($"/api/timelog/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
        }

        [Fact]
        public async Task CreateTimelog_WithoutUserIdHeader_ReturnsBadRequest()
        {
            var request = CreateRequest(HttpMethod.Post, "/api/timelog", userId: null, body: new
            {
                projectId = Guid.NewGuid(),
                taskId = Guid.NewGuid(),
                hoursSpent = 2,
                date = DateTime.Now.AddDays(-1)
            });

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateTimelog_WithFutureDate_ReturnsBadRequestFromRealValidationPipeline()
        {
            var request = CreateRequest(HttpMethod.Post, "/api/timelog", Guid.NewGuid(), new
            {
                projectId = Guid.NewGuid(),
                taskId = Guid.NewGuid(),
                hoursSpent = 2,
                date = DateTime.Now.AddDays(10)
            });

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("future", body, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateTimelog_WithZeroHours_ReturnsBadRequest()
        {
            var request = CreateRequest(HttpMethod.Post, "/api/timelog", Guid.NewGuid(), new
            {
                projectId = Guid.NewGuid(),
                taskId = Guid.NewGuid(),
                hoursSpent = 0,
                date = DateTime.Now.AddDays(-1)
            });

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateTimelog_ForNonexistentId_ReturnsNotFound()
        {
            var request = CreateRequest(HttpMethod.Put, $"/api/timelog/{Guid.NewGuid()}", Guid.NewGuid(), new { hoursSpent = 3 });

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateTimelog_WithoutUserIdHeader_ReturnsBadRequest()
        {
            var request = CreateRequest(HttpMethod.Put, $"/api/timelog/{Guid.NewGuid()}", body: new { hoursSpent = 3 });

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteTimelog_ForNonexistentId_ReturnsNotFound()
        {
            var request = CreateRequest(HttpMethod.Delete, $"/api/timelog/{Guid.NewGuid()}", Guid.NewGuid());

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteTimelog_WithoutUserIdHeader_ReturnsBadRequest()
        {
            var response = await _fx.Client.DeleteAsync($"/api/timelog/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateTimelog_ByNonOwner_ReturnsForbidden()
        {
            var createRequest = CreateRequest(HttpMethod.Post, "/api/timelog", _fx.KnownUserId, new
            {
                projectId = Guid.NewGuid(),
                taskId = _fx.KnownTaskId,
                hoursSpent = 1,
                date = DateTime.Now.AddDays(-1)
            });
            var created = await (await _fx.Client.SendAsync(createRequest)).Content.ReadFromJsonAsync<TimelogConfirmationDTO>();

            var updateRequest = CreateRequest(HttpMethod.Put, $"/api/timelog/{created!.Id}", Guid.NewGuid(), new { hoursSpent = 9 });
            var response = await _fx.Client.SendAsync(updateRequest);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            var stillOriginal = await (await _fx.Client.GetAsync($"/api/timelog/{created.Id}")).Content.ReadFromJsonAsync<TimelogDTO>();
            Assert.Equal(1, stillOriginal!.HoursSpent);
        }

        [Fact]
        public async Task DeleteTimelog_ByNonOwner_ReturnsForbiddenAndDoesNotDelete()
        {
            var createRequest = CreateRequest(HttpMethod.Post, "/api/timelog", _fx.KnownUserId, new
            {
                projectId = Guid.NewGuid(),
                taskId = _fx.KnownTaskId,
                hoursSpent = 1,
                date = DateTime.Now.AddDays(-1)
            });
            var created = await (await _fx.Client.SendAsync(createRequest)).Content.ReadFromJsonAsync<TimelogConfirmationDTO>();

            var deleteRequest = CreateRequest(HttpMethod.Delete, $"/api/timelog/{created!.Id}", Guid.NewGuid());
            var response = await _fx.Client.SendAsync(deleteRequest);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            var stillThere = await _fx.Client.GetAsync($"/api/timelog/{created.Id}");
            Assert.Equal(HttpStatusCode.OK, stillThere.StatusCode);
        }

        [Fact]
        public async Task GetTimelogs_FilteredByProjectId_ReturnsOnlyThatProject()
        {
            var projectId = Guid.NewGuid();
            var userId = _fx.KnownUserId;

            async Task Create(Guid pid)
            {
                var req = CreateRequest(HttpMethod.Post, "/api/timelog", userId, new
                {
                    projectId = pid,
                    taskId = Guid.NewGuid(),
                    hoursSpent = 1,
                    date = DateTime.Now.AddDays(-1)
                });
                var resp = await _fx.Client.SendAsync(req);
                Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
            }

            await Create(projectId);
            await Create(Guid.NewGuid());

            var response = await _fx.Client.GetAsync($"/api/timelog?projectId={projectId}");
            var list = await response.Content.ReadFromJsonAsync<List<TimelogDTO>>();

            Assert.Single(list!);
            Assert.Equal(projectId, list![0].ProjectId);
        }

        [Fact]
        public async Task GetTimelogs_WithNoResults_Returns200WithEmptyArrayNotNoContent()
        {
            var response = await _fx.Client.GetAsync($"/api/timelog?projectId={Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var list = await response.Content.ReadFromJsonAsync<List<TimelogDTO>>();
            Assert.NotNull(list);
            Assert.Empty(list!);
        }

        [Fact]
        public async Task CreateTimelog_WhenDependenciesAreUnreachable_StillSucceeds()
        {
            // Reproduces, end to end through real HTTP, the most severe bug found during
            // manual testing: unreachable Project/User/WorkPackage Service used to make
            // timelog creation fail (with data silently persisted anyway).
            using var factory = new TimelogApiFactory("http://localhost:1/", "http://localhost:2/", "http://localhost:3/", databaseName: "TimelogDB_integration_test_unreachable");
            using var client = factory.CreateClient();
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TimelogService.Context.TimelogContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var request = CreateRequest(HttpMethod.Post, "/api/timelog", Guid.NewGuid(), new
            {
                projectId = Guid.NewGuid(),
                taskId = Guid.NewGuid(),
                hoursSpent = 3,
                date = DateTime.Now.AddDays(-1)
            });
            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<TimelogConfirmationDTO>();
            Assert.Equal("Unknown User", created!.Username);
            Assert.Equal("Unknown Task", created.TaskTitle);

            context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task CreateTimelog_ForTaskThatDoesNotExist_ReturnsBadRequestAndDoesNotPersist()
        {
            var request = CreateRequest(HttpMethod.Post, "/api/timelog", _fx.KnownUserId, new
            {
                projectId = Guid.NewGuid(),
                taskId = _fx.MissingTaskId,
                hoursSpent = 2,
                date = DateTime.Now.AddDays(-1)
            });

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var list = await (await _fx.Client.GetAsync($"/api/timelog?taskId={_fx.MissingTaskId}"))
                .Content.ReadFromJsonAsync<List<TimelogDTO>>();
            Assert.Empty(list!);
        }

        [Fact]
        public async Task CreateTimelog_ForProjectThatDoesNotExist_ReturnsBadRequestAndDoesNotPersist_EvenAsAdmin()
        {
            var request = CreateRequest(HttpMethod.Post, "/api/timelog", _fx.AdminUserId, new
            {
                projectId = _fx.MissingProjectId,
                taskId = _fx.KnownTaskId,
                hoursSpent = 2,
                date = DateTime.Now.AddDays(-1)
            });

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var list = await (await _fx.Client.GetAsync($"/api/timelog?projectId={_fx.MissingProjectId}"))
                .Content.ReadFromJsonAsync<List<TimelogDTO>>();
            Assert.Empty(list!);
        }

        [Fact]
        public async Task CreateTimelog_ForProjectUserIsNotAMemberOf_ReturnsForbiddenAndDoesNotPersist()
        {
            var request = CreateRequest(HttpMethod.Post, "/api/timelog", _fx.KnownUserId, new
            {
                projectId = _fx.NonMemberProjectId,
                taskId = _fx.KnownTaskId,
                hoursSpent = 2,
                date = DateTime.Now.AddDays(-1)
            });

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            var list = await (await _fx.Client.GetAsync($"/api/timelog?projectId={_fx.NonMemberProjectId}"))
                .Content.ReadFromJsonAsync<List<TimelogDTO>>();
            Assert.Empty(list!);
        }

        [Fact]
        public async Task CreateTimelog_AsClient_ReturnsForbiddenAndDoesNotPersist()
        {
            var projectId = Guid.NewGuid();
            var request = CreateRequest(HttpMethod.Post, "/api/timelog", _fx.ClientUserId, new
            {
                projectId,
                taskId = _fx.KnownTaskId,
                hoursSpent = 2,
                date = DateTime.Now.AddDays(-1)
            });

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            var list = await (await _fx.Client.GetAsync($"/api/timelog?projectId={projectId}"))
                .Content.ReadFromJsonAsync<List<TimelogDTO>>();
            Assert.Empty(list!);
        }

        [Fact]
        public async Task CreateTimelog_AsAdminOnProjectTheyAreNotAMemberOf_StillSucceeds()
        {
            var request = CreateRequest(HttpMethod.Post, "/api/timelog", _fx.AdminUserId, new
            {
                projectId = Guid.NewGuid(),
                taskId = _fx.KnownTaskId,
                hoursSpent = 2,
                date = DateTime.Now.AddDays(-1)
            });

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<TimelogConfirmationDTO>();
            Assert.Equal("admin.user", created!.Username);
            Assert.Equal("admin.user@example.com", created.Email);
            Assert.Equal("Admin", created.UserRole);
        }
    }
}
