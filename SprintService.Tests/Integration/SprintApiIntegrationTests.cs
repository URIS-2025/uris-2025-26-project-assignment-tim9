using System.Net;
using System.Net.Http.Headers;
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
        public async Task CreateSprint_WithEndDateAfterProjectMilestone_ReturnsBadRequestAndDoesNotPersist()
        {
            // Uses the fixture's known project, whose fake Project Service milestone is due
            // SprintApiFixture.KnownExpectedDate (2026-09-01) - proves the cross-service
            // validation rule through the real HTTP pipeline, not just mocked at the repository
            // layer.
            var before = await _fx.Client.GetAsync($"/projects/{_fx.KnownProjectId}/sprints");
            var beforeCount = (await before.Content.ReadFromJsonAsync<List<SprintDTO>>())!.Count;

            var response = await _fx.Client.PostAsJsonAsync($"/projects/{_fx.KnownProjectId}/sprints", new
            {
                name = "Too Late Sprint",
                status = SprintStatus.NotStarted,
                startDate = new DateTime(2026, 8, 1),
                endDate = new DateTime(2026, 9, 15) // after the fixture's 2026-09-01 milestone due date
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("milestone", body, StringComparison.OrdinalIgnoreCase);

            var after = await _fx.Client.GetAsync($"/projects/{_fx.KnownProjectId}/sprints");
            var afterCount = (await after.Content.ReadFromJsonAsync<List<SprintDTO>>())!.Count;
            Assert.Equal(beforeCount, afterCount);
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
        public async Task UpdateSprint_WithEndDateAfterProjectMilestone_ReturnsBadRequestAndLeavesItUnchanged()
        {
            var createResponse = await _fx.Client.PostAsJsonAsync($"/projects/{_fx.KnownProjectId}/sprints", new
            {
                name = "Valid Sprint For Update Test",
                status = SprintStatus.NotStarted,
                startDate = new DateTime(2026, 8, 1),
                endDate = new DateTime(2026, 8, 15)
            });
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var created = await createResponse.Content.ReadFromJsonAsync<SprintConfirmationDTO>();

            var updateResponse = await _fx.Client.PutAsJsonAsync($"/sprints/{created!.Id}", new
            {
                projectId = _fx.KnownProjectId,
                name = "Pushed Too Far",
                status = SprintStatus.Active,
                startDate = new DateTime(2026, 8, 1),
                endDate = new DateTime(2026, 9, 15) // after the fixture's 2026-09-01 milestone due date
            });

            Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);

            var refetched = await (await _fx.Client.GetAsync($"/sprints/{created.Id}")).Content.ReadFromJsonAsync<SprintDTO>();
            Assert.Equal("Valid Sprint For Update Test", refetched!.Name);
            Assert.Equal(new DateTime(2026, 8, 15), refetched.EndDate);

            await _fx.Client.DeleteAsync($"/sprints/{created.Id}");
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
        public async Task UpdateSprint_ReassignedToNonexistentProject_ReturnsBadRequestAndLeavesItUnchanged()
        {
            // Own factory + fake server: the project the sprint is reassigned to via the update
            // body is confirmed 404, everything else (the original create) succeeds.
            var originalProjectId = Guid.NewGuid();
            var ghostProjectId = Guid.NewGuid();
            using var projectServer = new FakeJsonServer(request =>
            {
                var path = request.Url?.AbsolutePath.TrimStart('/') ?? "";
                if (path.StartsWith($"api/project/{ghostProjectId}", StringComparison.OrdinalIgnoreCase))
                {
                    return (404, null);
                }
                if (path.StartsWith("api/project/", StringComparison.OrdinalIgnoreCase))
                {
                    return (200, "{}");
                }
                return (204, null); // milestone lookups: exists, no milestones
            });

            using var factory = new SprintApiFactory(projectServer.BaseUrl, databaseName: "SprintDB_integration_test_reassign_ghost");
            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.ForRole("Admin"));
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SprintService.Context.SprintContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var createResponse = await client.PostAsJsonAsync($"/projects/{originalProjectId}/sprints", new
            {
                name = "Originally Valid Sprint",
                status = SprintStatus.NotStarted,
                startDate = new DateTime(2026, 1, 1),
                endDate = new DateTime(2026, 1, 15)
            });
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var created = await createResponse.Content.ReadFromJsonAsync<SprintConfirmationDTO>();

            var updateResponse = await client.PutAsJsonAsync($"/sprints/{created!.Id}", new
            {
                projectId = ghostProjectId,
                name = "Reassigned To Ghost",
                status = SprintStatus.Active,
                startDate = new DateTime(2026, 1, 1),
                endDate = new DateTime(2026, 1, 15)
            });

            Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);

            var refetched = await (await client.GetAsync($"/sprints/{created.Id}")).Content.ReadFromJsonAsync<SprintDTO>();
            Assert.Equal("Originally Valid Sprint", refetched!.Name);
            Assert.Equal(originalProjectId, refetched.ProjectId);

            context.Database.EnsureDeleted();
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
        public async Task GetSprintsForProject_WithNonexistentProject_ReturnsNotFound()
        {
            // Own factory + fake server confirming 404 - distinct from the shared fixture, where
            // every project ID is treated as real by design (see SprintApiFixture's comment).
            using var projectServer = new FakeJsonServer(_ => (404, null));
            using var factory = new SprintApiFactory(projectServer.BaseUrl, databaseName: "SprintDB_integration_test_get_nonexistent_project");
            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.ForRole("Admin"));
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SprintService.Context.SprintContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var response = await client.GetAsync($"/projects/{Guid.NewGuid()}/sprints");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task GetSprints_WithProjectIdQueryForNonexistentProject_ReturnsNotFound()
        {
            // Same check, via the ?projectId= convenience route instead of the required
            // /projects/{projectId}/sprints one - both have to agree.
            using var projectServer = new FakeJsonServer(_ => (404, null));
            using var factory = new SprintApiFactory(projectServer.BaseUrl, databaseName: "SprintDB_integration_test_get_query_nonexistent_project");
            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.ForRole("Admin"));
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SprintService.Context.SprintContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var response = await client.GetAsync($"/sprints?projectId={Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task GetSprintsForProject_WhenProjectServiceIsUnreachable_ReturnsBadRequest()
        {
            // Fail-closed, same principle as create/update: "can't verify" isn't grounds to
            // silently return whatever's in the local DB for that project ID.
            using var factory = new SprintApiFactory("http://localhost:1/", databaseName: "SprintDB_integration_test_get_unreachable");
            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.ForRole("Admin"));
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SprintService.Context.SprintContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var response = await client.GetAsync($"/projects/{Guid.NewGuid()}/sprints");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task CreateSprint_WhenProjectServiceIsUnreachable_FailsClosedAndPersistsNothing()
        {
            // Deliberately reversed from an earlier version of this test (which asserted the
            // opposite - that creation "still succeeds"). That was correct back when Project
            // Service enrichment was a best-effort nicety with nothing depending on it. Now that
            // a sprint's project must be positively confirmed to exist, being unable to reach
            // Project Service can no longer mean "assume it's fine" - it has to mean "reject",
            // otherwise a flaky/down dependency (or simply omitting a bearer token) would let
            // sprints attach to made-up projects. Uses its own factory pointed at a
            // guaranteed-closed port rather than the shared fixture's working fake server.
            using var factory = new SprintApiFactory("http://localhost:1/", databaseName: "SprintDB_integration_test_unreachable");
            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.ForRole("Admin"));
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SprintService.Context.SprintContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var response = await client.PostAsJsonAsync($"/projects/{Guid.NewGuid()}/sprints", new
            {
                name = "Unverifiable Sprint",
                status = SprintStatus.NotStarted,
                startDate = new DateTime(2026, 1, 1),
                endDate = new DateTime(2026, 1, 15)
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            // The DB isn't actually empty - SprintContext seeds one row via HasData - so check
            // for absence of this specific sprint rather than an empty table.
            Assert.DoesNotContain(context.Sprints, s => s.Name == "Unverifiable Sprint");

            context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task CreateSprint_WithNonexistentProject_ReturnsBadRequestAndPersistsNothing()
        {
            // Own factory + fake server that positively confirms "404, this project doesn't
            // exist" - distinct from the "can't tell at all" case above, but rejected the same
            // way (both are SprintValidationException -> 400).
            using var projectServer = new FakeJsonServer(_ => (404, null));
            using var factory = new SprintApiFactory(projectServer.BaseUrl, databaseName: "SprintDB_integration_test_nonexistent_project");
            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.ForRole("Admin"));
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SprintService.Context.SprintContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var response = await client.PostAsJsonAsync($"/projects/{Guid.NewGuid()}/sprints", new
            {
                name = "Ghost Project Sprint",
                status = SprintStatus.NotStarted,
                startDate = new DateTime(2026, 1, 1),
                endDate = new DateTime(2026, 1, 15)
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("does not exist", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(context.Sprints, s => s.Name == "Ghost Project Sprint");

            context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task CreateSprint_ForwardsCallersAuthorizationHeaderToProjectService()
        {
            // Project Service's endpoints require a JWT ([Authorize]) - SprintService forwards
            // whatever bearer token the caller sent it (AuthForwardingHandler). Uses a real
            // minted token, not a placeholder string, since SprintService now validates its own
            // inbound token too - this has to pass both gates to prove forwarding end to end.
            var bearerToken = $"Bearer {TestTokens.ForRole("Admin")}";
            string? capturedAuthHeader = null;
            var milestoneId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var expectedDate = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

            using var projectServer = new FakeJsonServer(request =>
            {
                capturedAuthHeader = request.Headers["Authorization"];
                return (200, $"[{{\"milestoneId\":\"{milestoneId}\",\"projectId\":\"{projectId}\",\"expectedDate\":\"{expectedDate:O}\"}}]");
            });

            using var factory = new SprintApiFactory(projectServer.BaseUrl, databaseName: "SprintDB_integration_test_authforward");
            using var client = factory.CreateClient();
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SprintService.Context.SprintContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            using var request = new HttpRequestMessage(HttpMethod.Post, $"/projects/{projectId}/sprints")
            {
                Content = JsonContent.Create(new
                {
                    name = "Authed Sprint",
                    status = SprintStatus.NotStarted,
                    startDate = new DateTime(2026, 1, 1),
                    endDate = new DateTime(2026, 1, 15)
                })
            };
            request.Headers.TryAddWithoutValidation("Authorization", bearerToken);

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<SprintConfirmationDTO>();
            Assert.Equal(milestoneId, created!.MilestoneId);
            Assert.Equal(expectedDate, created.ExpectedDate);
            Assert.Equal(bearerToken, capturedAuthHeader);

            context.Database.EnsureDeleted();
        }

        // CreateSprint_WithNoIncomingAuthHeader_CallsProjectServiceWithoutOne used to live here,
        // proving a request with no Authorization header still reached Project Service (just
        // without forwarding a token). Its premise no longer holds: SprintController now carries
        // [Authorize], so a request with no token never reaches the repository/forwarding logic
        // at all - it gets a 401 straight from SprintService's own auth gate. See
        // CreateSprint_WithNoToken_ReturnsUnauthorized below for that behavior instead.

        // ---------- Role-based authorization ----------

        [Fact]
        public async Task CreateSprint_WithNoToken_ReturnsUnauthorized()
        {
            // Own client with no default Authorization header - _fx.Client always carries one
            // (Admin, so every other test keeps working), and simply omitting the header on a
            // request sent through it wouldn't actually omit it: HttpClient fills in any header
            // a request doesn't already set from DefaultRequestHeaders.
            using var projectServer = new FakeJsonServer(_ => (200, "{}"));
            using var factory = new SprintApiFactory(projectServer.BaseUrl, databaseName: "SprintDB_integration_test_no_token");
            using var client = factory.CreateClient();
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SprintService.Context.SprintContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var response = await client.PostAsJsonAsync($"/projects/{Guid.NewGuid()}/sprints", new
            {
                name = "No Token Sprint",
                status = SprintStatus.NotStarted,
                startDate = new DateTime(2026, 1, 1),
                endDate = new DateTime(2026, 1, 15)
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            context.Database.EnsureDeleted();
        }

        [Theory]
        [InlineData("TeamMember")]
        [InlineData("Client")]
        public async Task CreateSprint_WithReadOnlyRole_ReturnsForbidden(string role)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/projects/{Guid.NewGuid()}/sprints")
            {
                Content = JsonContent.Create(new
                {
                    name = "Read Only Role Sprint",
                    status = SprintStatus.NotStarted,
                    startDate = new DateTime(2026, 1, 1),
                    endDate = new DateTime(2026, 1, 15)
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.ForRole(role));

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData("TeamMember")]
        [InlineData("Client")]
        public async Task DeleteSprint_WithReadOnlyRole_ReturnsForbiddenAndDoesNotDelete(string role)
        {
            var createResponse = await _fx.Client.PostAsJsonAsync($"/projects/{_fx.KnownProjectId}/sprints", new
            {
                name = "Protected From Delete",
                status = SprintStatus.NotStarted,
                startDate = new DateTime(2026, 1, 1),
                endDate = new DateTime(2026, 1, 15)
            });
            var created = await createResponse.Content.ReadFromJsonAsync<SprintConfirmationDTO>();

            using var request = new HttpRequestMessage(HttpMethod.Delete, $"/sprints/{created!.Id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.ForRole(role));
            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var stillThere = await _fx.Client.GetAsync($"/sprints/{created.Id}");
            Assert.Equal(HttpStatusCode.OK, stillThere.StatusCode);

            await _fx.Client.DeleteAsync($"/sprints/{created.Id}"); // cleanup as Admin
        }

        [Fact]
        public async Task GetSprints_WithTeamMemberRole_SeesEverythingUnscoped()
        {
            // TeamMember gets read access to every sprint regardless of project - same as
            // Admin/ProjectManager for GET purposes, just without write access.
            using var request = new HttpRequestMessage(HttpMethod.Get, "/sprints");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.ForRole("TeamMember"));

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetSprints_WithClientRole_OnlySeesSprintsForTheirOwnProjects()
        {
            // Own factory + fake server: Project Service's GET api/project/user/{userId} says
            // this Client belongs to exactly one project - proves the filtering actually narrows
            // results through the real HTTP pipeline, not just mocked at the repository layer.
            var clientUserId = Guid.NewGuid();
            var ownProjectId = Guid.NewGuid();
            var otherProjectId = Guid.NewGuid();

            using var projectServer = new FakeJsonServer(request =>
            {
                var path = request.Url?.AbsolutePath.TrimStart('/') ?? "";
                if (path.StartsWith($"api/project/user/{clientUserId}", StringComparison.OrdinalIgnoreCase))
                {
                    return (200, $"[{{\"projectId\":\"{ownProjectId}\"}}]");
                }
                if (path.StartsWith("api/project/", StringComparison.OrdinalIgnoreCase))
                {
                    return (200, "{}"); // existence checks during setup: every project is real
                }
                return (204, null); // milestone lookups: exists, no milestones
            });

            using var factory = new SprintApiFactory(projectServer.BaseUrl, databaseName: "SprintDB_integration_test_client_scoping");
            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.ForRole("Admin"));
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SprintService.Context.SprintContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            async Task Create(Guid pid, string name)
            {
                var resp = await client.PostAsJsonAsync($"/projects/{pid}/sprints", new
                {
                    name,
                    status = SprintStatus.NotStarted,
                    startDate = new DateTime(2026, 1, 1),
                    endDate = new DateTime(2026, 1, 15)
                });
                Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
            }

            await Create(ownProjectId, "Client's own sprint");
            await Create(otherProjectId, "Someone else's sprint");

            using var clientRequest = new HttpRequestMessage(HttpMethod.Get, "/sprints");
            clientRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.ForRole("Client", clientUserId));
            var clientResponse = await client.SendAsync(clientRequest);

            Assert.Equal(HttpStatusCode.OK, clientResponse.StatusCode);
            var list = await clientResponse.Content.ReadFromJsonAsync<List<SprintDTO>>();
            Assert.Single(list!);
            Assert.Equal("Client's own sprint", list![0].Name);
            Assert.Equal(ownProjectId, list[0].ProjectId);

            context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task GetSprintById_WithClientRoleNotOwningIt_ReturnsNotFound()
        {
            // Same scenario as above but for the single-item route: a Client who isn't a member
            // of the sprint's project gets 404, indistinguishable from it not existing at all.
            var clientUserId = Guid.NewGuid();
            var otherProjectId = Guid.NewGuid();

            using var projectServer = new FakeJsonServer(request =>
            {
                var path = request.Url?.AbsolutePath.TrimStart('/') ?? "";
                if (path.StartsWith($"api/project/user/{clientUserId}", StringComparison.OrdinalIgnoreCase))
                {
                    return (200, "[]"); // this Client belongs to no projects
                }
                if (path.StartsWith("api/project/", StringComparison.OrdinalIgnoreCase))
                {
                    return (200, "{}");
                }
                return (204, null);
            });

            using var factory = new SprintApiFactory(projectServer.BaseUrl, databaseName: "SprintDB_integration_test_client_by_id");
            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.ForRole("Admin"));
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SprintService.Context.SprintContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var createResponse = await client.PostAsJsonAsync($"/projects/{otherProjectId}/sprints", new
            {
                name = "Not The Client's Sprint",
                status = SprintStatus.NotStarted,
                startDate = new DateTime(2026, 1, 1),
                endDate = new DateTime(2026, 1, 15)
            });
            var created = await createResponse.Content.ReadFromJsonAsync<SprintConfirmationDTO>();

            using var clientRequest = new HttpRequestMessage(HttpMethod.Get, $"/sprints/{created!.Id}");
            clientRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.ForRole("Client", clientUserId));
            var clientResponse = await client.SendAsync(clientRequest);

            Assert.Equal(HttpStatusCode.NotFound, clientResponse.StatusCode);

            context.Database.EnsureDeleted();
        }
    }
}
