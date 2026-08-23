using System.Net;
using System.Text.Json;
using SprintService.ServiceCalls.Project;

namespace SprintService.Tests
{
    /// <summary>
    /// Unit tests for the concrete ProjectService HTTP client - the milestone endpoint/shape and
    /// "which milestone wins" selection logic that IProjectService mocks elsewhere in the suite
    /// (e.g. SprintRepositoryTests) intentionally bypass.
    /// </summary>
    public class ProjectServiceTests
    {
        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

            public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
            {
                _respond = respond;
            }

            public HttpRequestMessage? LastRequest { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                return Task.FromResult(_respond(request));
            }
        }

        private static HttpClient CreateClient(StubHandler handler) =>
            new(handler) { BaseAddress = new Uri("https://project-service.test/") };

        [Fact]
        public async Task GetProjectByIdAsync_CallsMilestoneByProjectEndpoint_NotProjectEndpoint()
        {
            var projectId = Guid.NewGuid();
            var handler = new StubHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") });
            var sut = new ProjectService(CreateClient(handler));

            await sut.GetProjectByIdAsync(projectId);

            Assert.Equal($"/api/milestone/project/{projectId}", handler.LastRequest!.RequestUri!.AbsolutePath);
        }

        [Fact]
        public async Task GetProjectByIdAsync_WithMultipleMilestones_PicksSoonestUpcomingOne()
        {
            var projectId = Guid.NewGuid();
            var past = new { milestoneId = Guid.NewGuid(), projectId, expectedDate = DateTime.UtcNow.AddDays(-30) };
            var soonest = new { milestoneId = Guid.NewGuid(), projectId, expectedDate = DateTime.UtcNow.AddDays(10) };
            var later = new { milestoneId = Guid.NewGuid(), projectId, expectedDate = DateTime.UtcNow.AddDays(60) };
            var json = JsonSerializer.Serialize(new[] { later, past, soonest });

            var handler = new StubHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
            var sut = new ProjectService(CreateClient(handler));

            var result = await sut.GetProjectByIdAsync(projectId);

            Assert.NotNull(result);
            Assert.Equal(soonest.milestoneId, result!.MilestoneID);
        }

        [Fact]
        public async Task GetProjectByIdAsync_WithOnlyPastMilestones_FallsBackToSoonestOverall()
        {
            var projectId = Guid.NewGuid();
            var older = new { milestoneId = Guid.NewGuid(), projectId, expectedDate = DateTime.UtcNow.AddDays(-60) };
            var newer = new { milestoneId = Guid.NewGuid(), projectId, expectedDate = DateTime.UtcNow.AddDays(-10) };
            var json = JsonSerializer.Serialize(new[] { older, newer });

            var handler = new StubHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
            var sut = new ProjectService(CreateClient(handler));

            var result = await sut.GetProjectByIdAsync(projectId);

            Assert.NotNull(result);
            Assert.Equal(newer.milestoneId, result!.MilestoneID);
        }

        [Theory]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task GetProjectByIdAsync_OnErrorStatus_ReturnsNullInsteadOfThrowing(HttpStatusCode status)
        {
            var handler = new StubHandler(_ => new HttpResponseMessage(status));
            var sut = new ProjectService(CreateClient(handler));

            var result = await sut.GetProjectByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetProjectByIdAsync_WithNoContentForProjectWithNoMilestones_ReturnsNull()
        {
            // Mirrors MilestoneController.GetMilestonesByProjectId's real behavior for a project
            // with zero milestones: 204 No Content, empty body.
            var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
            var sut = new ProjectService(CreateClient(handler));

            var result = await sut.GetProjectByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetProjectByIdAsync_WithMalformedJson_ReturnsNullInsteadOfThrowing()
        {
            var handler = new StubHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("not json") });
            var sut = new ProjectService(CreateClient(handler));

            var result = await sut.GetProjectByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }
    }
}
