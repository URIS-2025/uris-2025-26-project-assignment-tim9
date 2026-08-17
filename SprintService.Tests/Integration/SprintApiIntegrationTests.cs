using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using SprintService.Models.DTO;
using SprintService.Models.Enums;

namespace SprintService.Tests.Integration
{
    /// <summary>
    /// Exercises the real HTTP pipeline (routing, model binding/validation, DI, controller,
    /// repository) against real local MySQL, plus a fake HTTP stand-in for Project Service.
    /// Requires a local MySQL reachable on localhost:3306 (root/root).
    /// </summary>
    public class SprintApiIntegrationTests : IClassFixture<SprintApiFixture>
    {
        private readonly SprintApiFixture _fx;

        public SprintApiIntegrationTests(SprintApiFixture fixture)
        {
            _fx = fixture;
        }

        [Fact]
        public async Task FullLifecycle_CreateGetUpdateDelete_WorksEndToEnd()
        {
            // 1) Create under the project (POST /projects/{projectId}/sprints), with Project
            // Service recognizing this project - proves real milestone enrichment through the
            // whole Controller -> Repository -> IProjectService -> HttpClient -> fake server ->
            // JSON deserialize chain.
            var createResponse = await _fx.Client.PostAsJsonAsync($"/projects/{_fx.KnownProjectId}/sprints", new
            {
                name = "Integration Sprint",
                status = SprintStatus.NotStarted,
                startDate = new DateTime(2026, 1, 1),
                endDate = new DateTime(2026, 1, 15)
            });
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            Assert.NotNull(createResponse.Headers.Location);
            var created = await createResponse.Content.ReadFromJsonAsync<SprintConfirmationDTO>();
            Assert.NotNull(created);
            Assert.Equal(_fx.KnownMilestoneId, created!.MilestoneId);
            Assert.Equal(SprintApiFixture.KnownExpectedDate, created.ExpectedDate);

            // 2) Get it back (GET /sprints/{sprintId})
            var getResponse = await _fx.Client.GetAsync($"/sprints/{created.Id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var fetched = await getResponse.Content.ReadFromJsonAsync<SprintDTO>();
            Assert.Equal("Integration Sprint", fetched!.Name);
            Assert.Equal(_fx.KnownProjectId, fetched.ProjectId);

            // 3) Update (PUT /sprints/{sprintId}) - the actual bug found during manual testing
            // was that this silently changed nothing while still returning 200. Prove it really
            // applies now.
            var updateResponse = await _fx.Client.PutAsJsonAsync($"/sprints/{created.Id}", new
            {
                projectId = _fx.KnownProjectId,
                name = "Renamed Sprint",
                status = SprintStatus.Active,
                startDate = new DateTime(2026, 2, 1),
                endDate = new DateTime(2026, 2, 15)
            });
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            var updated = await updateResponse.Content.ReadFromJsonAsync<SprintConfirmationDTO>();
            Assert.Equal("Renamed Sprint", updated!.Name);
            Assert.Equal(SprintStatus.Active, updated.Status);

            var refetched = await (await _fx.Client.GetAsync($"/sprints/{created.Id}")).Content.ReadFromJsonAsync<SprintDTO>();
            Assert.Equal("Renamed Sprint", refetched!.Name);
            Assert.Equal(SprintStatus.Active, refetched.Status);
            Assert.Equal(new DateTime(2026, 2, 1), refetched.StartDate);

            // 4) Delete (DELETE /sprints/{sprintId}), then confirm it's really gone
            var deleteResponse = await _fx.Client.DeleteAsync($"/sprints/{created.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getAfterDelete = await _fx.Client.GetAsync($"/sprints/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
        }

        [Fact]
        public async Task CreateSprint_WithEndDateBeforeStartDate_ReturnsBadRequestFromRealValidationPipeline()
        {
            var response = await _fx.Client.PostAsJsonAsync($"/projects/{Guid.NewGuid()}/sprints", new
            {
                name = "Bad Dates",
                status = SprintStatus.NotStarted,
                startDate = new DateTime(2026, 1, 15),
                endDate = new DateTime(2026, 1, 1)
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("End date must be after start date", body);
        }

        [Fact]
        public async Task CreateSprint_WithShortName_ReturnsBadRequest()
        {
            var response = await _fx.Client.PostAsJsonAsync($"/projects/{Guid.NewGuid()}/sprints", new
            {
                name = "ab",
                status = SprintStatus.NotStarted,
                startDate = new DateTime(2026, 1, 1),
                endDate = new DateTime(2026, 1, 15)
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateSprint_TakesProjectIdFromRouteNotBody()
        {
            var routeProjectId = Guid.NewGuid();

            var response = await _fx.Client.PostAsJsonAsync($"/projects/{routeProjectId}/sprints", new
            {
                name = "Route Wins",
                status = SprintStatus.NotStarted,
                startDate = new DateTime(2026, 1, 1),
                endDate = new DateTime(2026, 1, 15)
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<SprintConfirmationDTO>();
            var fetched = await (await _fx.Client.GetAsync($"/sprints/{created!.Id}")).Content.ReadFromJsonAsync<SprintDTO>();
            Assert.Equal(routeProjectId, fetched!.ProjectId);
        }

        [Fact]
        public async Task UpdateSprint_ForNonexistentId_ReturnsNotFound()
        {
            var response = await _fx.Client.PutAsJsonAsync($"/sprints/{Guid.NewGuid()}", new
            {
                projectId = Guid.NewGuid(),
                name = "Ghost Sprint",
                status = SprintStatus.NotStarted,
                startDate = new DateTime(2026, 1, 1),
                endDate = new DateTime(2026, 1, 15)
            });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteSprint_ForNonexistentId_ReturnsNotFound()
        {
            var response = await _fx.Client.DeleteAsync($"/sprints/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetSprintsForProject_ReturnsOnlyThatProject()
        {
            var projectId = Guid.NewGuid();
            var otherProjectId = Guid.NewGuid();

            async Task Create(Guid pid, string name)
            {
                var resp = await _fx.Client.PostAsJsonAsync($"/projects/{pid}/sprints", new
                {
                    name,
                    status = SprintStatus.NotStarted,
                    startDate = new DateTime(2026, 1, 1),
                    endDate = new DateTime(2026, 1, 15)
                });
                Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
            }

            await Create(projectId, "In project");
            await Create(otherProjectId, "Other project");

            // Required route
            var response = await _fx.Client.GetAsync($"/projects/{projectId}/sprints");
            var list = await response.Content.ReadFromJsonAsync<List<SprintDTO>>();

            Assert.Single(list!);
            Assert.Equal("In project", list![0].Name);

            // The extra query-filter convenience route should agree with it
            var altResponse = await _fx.Client.GetAsync($"/sprints?projectId={projectId}");
            var altList = await altResponse.Content.ReadFromJsonAsync<List<SprintDTO>>();
            Assert.Single(altList!);
            Assert.Equal("In project", altList![0].Name);
        }

        [Fact]
        public async Task GetSprints_WithNoResults_Returns200WithEmptyArrayNotNoContent()
        {
            var response = await _fx.Client.GetAsync($"/projects/{Guid.NewGuid()}/sprints");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var list = await response.Content.ReadFromJsonAsync<List<SprintDTO>>();
            Assert.NotNull(list);
            Assert.Empty(list!);
        }

        [Fact]
        public async Task CreateSprint_WhenProjectServiceIsUnreachable_StillSucceeds()
        {
            // Reproduces, end to end through real HTTP, the most severe bug found during
            // manual testing: an unreachable Project Service used to make sprint creation
            // fail (with data silently persisted anyway). Uses its own factory pointed at a
            // guaranteed-closed port rather than the shared fixture's working fake server.
            using var factory = new SprintApiFactory("http://localhost:1/", databaseName: "SprintDB_integration_test_unreachable");
            using var client = factory.CreateClient();
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SprintService.Context.SprintContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var response = await client.PostAsJsonAsync($"/projects/{Guid.NewGuid()}/sprints", new
            {
                name = "Resilient Sprint",
                status = SprintStatus.NotStarted,
                startDate = new DateTime(2026, 1, 1),
                endDate = new DateTime(2026, 1, 15)
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<SprintConfirmationDTO>();
            Assert.Equal("Resilient Sprint", created!.Name);
            Assert.Null(created.MilestoneId);

            context.Database.EnsureDeleted();
        }
    }
}
