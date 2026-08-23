using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SprintService.Controllers;
using SprintService.Data;
using SprintService.Models.DTO;
using SprintService.Models.Enums;

namespace SprintService.Tests
{
    public class SprintControllerTests
    {
        private static SprintDTO SampleDto(Guid? id = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            Name = "Sample Sprint",
            Status = SprintStatus.Active,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 15)
        };

        /// <summary>
        /// GetClientUserId() reads User off ControllerContext, which a bare `new
        /// SprintController(...)` never sets - role defaults to Admin (unrestricted), matching
        /// every existing test's original assumption unless a test asks for something else.
        /// [Authorize] itself isn't enforced here - that's an MVC filter, only exercised via
        /// SprintApiIntegrationTests (WebApplicationFactory), not direct action invocation.
        /// </summary>
        private static SprintController CreateController(ISprintRepository repo, string role = "Admin", Guid? userId = null)
        {
            var claims = new List<Claim> { new(ClaimTypes.Role, role) };
            if (userId is not null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
            }

            return new SprintController(repo)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                    }
                }
            };
        }

        // ---------- GetSprints ----------

        [Fact]
        public async Task GetSprints_ReturnsOkWithRepositoryResult()
        {
            var repo = new Mock<ISprintRepository>();
            var expected = new List<SprintDTO> { SampleDto() };
            repo.Setup(r => r.GetSprintsForCallerAsync(null, null)).ReturnsAsync(expected);
            var controller = CreateController(repo.Object);

            var result = await controller.GetSprints(null);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, ok.Value);
        }

        [Fact]
        public async Task GetSprints_WithEmptyResult_StillReturnsOkNotNoContent()
        {
            // Reproduces a real fix: this used to return 204 for an empty list, which has no
            // body and can break strongly-typed clients expecting to deserialize a JSON array.
            var repo = new Mock<ISprintRepository>();
            repo.Setup(r => r.GetSprintsForCallerAsync(null, null)).ReturnsAsync(new List<SprintDTO>());
            var controller = CreateController(repo.Object);

            var result = await controller.GetSprints(null);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Empty((IEnumerable<SprintDTO>)ok.Value!);
        }

        [Fact]
        public async Task GetSprints_PassesProjectIdFilterThrough()
        {
            var repo = new Mock<ISprintRepository>();
            var projectId = Guid.NewGuid();
            repo.Setup(r => r.GetSprintsForCallerAsync(projectId, null)).ReturnsAsync(new List<SprintDTO>());
            var controller = CreateController(repo.Object);

            await controller.GetSprints(projectId);

            repo.Verify(r => r.GetSprintsForCallerAsync(projectId, null), Times.Once);
        }

        [Fact]
        public async Task GetSprints_ForClientRole_PassesTheirUserIdAsScopeToRepository()
        {
            var repo = new Mock<ISprintRepository>();
            var clientUserId = Guid.NewGuid();
            repo.Setup(r => r.GetSprintsForCallerAsync(null, clientUserId)).ReturnsAsync(new List<SprintDTO>());
            var controller = CreateController(repo.Object, role: "Client", userId: clientUserId);

            await controller.GetSprints(null);

            repo.Verify(r => r.GetSprintsForCallerAsync(null, clientUserId), Times.Once);
        }

        [Fact]
        public async Task GetSprints_ForTeamMemberRole_PassesNoScopeToRepository()
        {
            // TeamMember can view every sprint regardless of project, same as Admin/ProjectManager
            // - only Client gets scoped.
            var repo = new Mock<ISprintRepository>();
            repo.Setup(r => r.GetSprintsForCallerAsync(null, null)).ReturnsAsync(new List<SprintDTO>());
            var controller = CreateController(repo.Object, role: "TeamMember");

            await controller.GetSprints(null);

            repo.Verify(r => r.GetSprintsForCallerAsync(null, null), Times.Once);
        }

        [Fact]
        public async Task GetSprints_ForClientRoleWithNoUserIdClaim_ScopesToEmptyGuidRatherThanUnscoped()
        {
            // A Client token missing/malformed "sub" must not accidentally fall through to
            // unscoped (null) access - it has to still deny by resolving to an empty-project scope.
            var repo = new Mock<ISprintRepository>();
            repo.Setup(r => r.GetSprintsForCallerAsync(null, Guid.Empty)).ReturnsAsync(new List<SprintDTO>());
            var controller = CreateController(repo.Object, role: "Client"); // no userId supplied

            await controller.GetSprints(null);

            repo.Verify(r => r.GetSprintsForCallerAsync(null, Guid.Empty), Times.Once);
        }

        // ---------- GetSprintsForProject (required route: GET /projects/{projectId}/sprints) ----------

        [Fact]
        public async Task GetSprintsForProject_PassesProjectIdThrough()
        {
            var repo = new Mock<ISprintRepository>();
            var projectId = Guid.NewGuid();
            var expected = new List<SprintDTO> { SampleDto() };
            repo.Setup(r => r.GetSprintsForCallerAsync(projectId, null)).ReturnsAsync(expected);
            var controller = CreateController(repo.Object);

            var result = await controller.GetSprintsForProject(projectId);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, ok.Value);
        }

        [Fact]
        public async Task GetSprintsForProject_WhenProjectDoesNotExist_ReturnsNotFound()
        {
            var repo = new Mock<ISprintRepository>();
            var projectId = Guid.NewGuid();
            repo.Setup(r => r.GetSprintsForCallerAsync(projectId, null)).ThrowsAsync(new ProjectNotFoundException(projectId));
            var controller = CreateController(repo.Object);

            var result = await controller.GetSprintsForProject(projectId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetSprints_WhenProjectDoesNotExist_ReturnsNotFound()
        {
            var repo = new Mock<ISprintRepository>();
            var projectId = Guid.NewGuid();
            repo.Setup(r => r.GetSprintsForCallerAsync(projectId, null)).ThrowsAsync(new ProjectNotFoundException(projectId));
            var controller = CreateController(repo.Object);

            var result = await controller.GetSprints(projectId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetSprints_WhenProjectExistenceIsUnknown_ReturnsBadRequestWithMessage()
        {
            var repo = new Mock<ISprintRepository>();
            var projectId = Guid.NewGuid();
            repo.Setup(r => r.GetSprintsForCallerAsync(projectId, null))
                .ThrowsAsync(new SprintValidationException("Could not verify that project exists."));
            var controller = CreateController(repo.Object);

            var result = await controller.GetSprints(projectId);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Could not verify that project exists.", badRequest.Value);
        }

        // ---------- GetSprintById ----------

        [Fact]
        public async Task GetSprintById_WhenFound_ReturnsOk()
        {
            var repo = new Mock<ISprintRepository>();
            var dto = SampleDto();
            repo.Setup(r => r.GetSprintByIdForCallerAsync(dto.Id, null)).ReturnsAsync(dto);
            var controller = CreateController(repo.Object);

            var result = await controller.GetSprintById(dto.Id);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(dto, ok.Value);
        }

        [Fact]
        public async Task GetSprintById_WhenMissing_ReturnsNotFound()
        {
            var repo = new Mock<ISprintRepository>();
            repo.Setup(r => r.GetSprintByIdForCallerAsync(It.IsAny<Guid>(), null)).ReturnsAsync((SprintDTO?)null);
            var controller = CreateController(repo.Object);

            var result = await controller.GetSprintById(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetSprintById_ForClientRoleNotOwningIt_ReturnsNotFound()
        {
            // GetSprintByIdForCallerAsync collapses "doesn't exist" and "exists but not theirs"
            // into the same null - deliberately not distinguished, so a Client can't probe for
            // other projects' sprint IDs.
            var repo = new Mock<ISprintRepository>();
            var sprintId = Guid.NewGuid();
            var clientUserId = Guid.NewGuid();
            repo.Setup(r => r.GetSprintByIdForCallerAsync(sprintId, clientUserId)).ReturnsAsync((SprintDTO?)null);
            var controller = CreateController(repo.Object, role: "Client", userId: clientUserId);

            var result = await controller.GetSprintById(sprintId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // ---------- CreateSprint (required route: POST /projects/{projectId}/sprints) ----------

        [Fact]
        public async Task CreateSprint_ReturnsCreatedAtRouteWithLocation()
        {
            // Reproduces a real fix: this used to return Created("", ...) - an empty Location
            // header that doesn't actually point at the new resource.
            var repo = new Mock<ISprintRepository>();
            var projectId = Guid.NewGuid();
            var confirmation = new SprintConfirmationDTO { Id = Guid.NewGuid(), Name = "New Sprint" };
            repo.Setup(r => r.CreateSprintAsync(projectId, It.IsAny<SprintCreationDTO>())).ReturnsAsync(confirmation);
            var controller = new SprintController(repo.Object);
            var dto = new SprintCreationDTO { Name = "New Sprint" };

            var result = await controller.CreateSprint(projectId, dto);

            var created = Assert.IsType<CreatedAtRouteResult>(result.Result);
            Assert.Equal("GetSprintById", created.RouteName);
            Assert.Equal(confirmation.Id, created.RouteValues!["sprintId"]);
            Assert.Same(confirmation, created.Value);
        }

        [Fact]
        public async Task CreateSprint_PassesProjectIdFromRouteToRepository()
        {
            var repo = new Mock<ISprintRepository>();
            var projectId = Guid.NewGuid();
            repo.Setup(r => r.CreateSprintAsync(projectId, It.IsAny<SprintCreationDTO>()))
                .ReturnsAsync(new SprintConfirmationDTO { Id = Guid.NewGuid() });
            var controller = new SprintController(repo.Object);

            await controller.CreateSprint(projectId, new SprintCreationDTO { Name = "X" });

            repo.Verify(r => r.CreateSprintAsync(projectId, It.IsAny<SprintCreationDTO>()), Times.Once);
        }

        [Fact]
        public async Task CreateSprint_WhenRepositoryThrowsValidationException_ReturnsBadRequestWithMessage()
        {
            var repo = new Mock<ISprintRepository>();
            var projectId = Guid.NewGuid();
            repo.Setup(r => r.CreateSprintAsync(projectId, It.IsAny<SprintCreationDTO>()))
                .ThrowsAsync(new SprintValidationException("Sprint end date is after the milestone due date."));
            var controller = new SprintController(repo.Object);

            var result = await controller.CreateSprint(projectId, new SprintCreationDTO { Name = "Too Late" });

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Sprint end date is after the milestone due date.", badRequest.Value);
        }

        // ---------- UpdateSprint (required route: PUT /sprints/{sprintId}) ----------

        [Fact]
        public async Task UpdateSprint_WhenFound_ReturnsOk()
        {
            var repo = new Mock<ISprintRepository>();
            var sprintId = Guid.NewGuid();
            var confirmation = new SprintConfirmationDTO { Id = sprintId, Name = "Updated" };
            repo.Setup(r => r.UpdateSprintAsync(sprintId, It.IsAny<SprintUpdateDTO>())).ReturnsAsync(confirmation);
            var controller = new SprintController(repo.Object);
            var dto = new SprintUpdateDTO { Name = "Updated", ProjectId = Guid.NewGuid() };

            var result = await controller.UpdateSprint(sprintId, dto);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(confirmation, ok.Value);
        }

        [Fact]
        public async Task UpdateSprint_WhenMissing_ReturnsNotFound()
        {
            var repo = new Mock<ISprintRepository>();
            repo.Setup(r => r.UpdateSprintAsync(It.IsAny<Guid>(), It.IsAny<SprintUpdateDTO>())).ReturnsAsync((SprintConfirmationDTO?)null);
            var controller = new SprintController(repo.Object);
            var dto = new SprintUpdateDTO { Name = "Ghost", ProjectId = Guid.NewGuid() };

            var result = await controller.UpdateSprint(Guid.NewGuid(), dto);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdateSprint_PassesSprintIdFromRouteToRepository()
        {
            var repo = new Mock<ISprintRepository>();
            var sprintId = Guid.NewGuid();
            repo.Setup(r => r.UpdateSprintAsync(sprintId, It.IsAny<SprintUpdateDTO>()))
                .ReturnsAsync(new SprintConfirmationDTO { Id = sprintId });
            var controller = new SprintController(repo.Object);

            await controller.UpdateSprint(sprintId, new SprintUpdateDTO { Name = "X", ProjectId = Guid.NewGuid() });

            repo.Verify(r => r.UpdateSprintAsync(sprintId, It.IsAny<SprintUpdateDTO>()), Times.Once);
        }

        [Fact]
        public async Task UpdateSprint_WhenRepositoryThrowsValidationException_ReturnsBadRequestWithMessage()
        {
            var repo = new Mock<ISprintRepository>();
            var sprintId = Guid.NewGuid();
            repo.Setup(r => r.UpdateSprintAsync(sprintId, It.IsAny<SprintUpdateDTO>()))
                .ThrowsAsync(new SprintValidationException("Sprint end date is after the milestone due date."));
            var controller = new SprintController(repo.Object);
            var dto = new SprintUpdateDTO { Name = "Too Late", ProjectId = Guid.NewGuid() };

            var result = await controller.UpdateSprint(sprintId, dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Sprint end date is after the milestone due date.", badRequest.Value);
        }

        // ---------- DeleteSprint ----------

        [Fact]
        public void DeleteSprint_WhenFound_ReturnsNoContentAndCallsRepository()
        {
            var repo = new Mock<ISprintRepository>();
            var id = Guid.NewGuid();
            repo.Setup(r => r.GetSprintById(id)).Returns(SampleDto(id));
            var controller = new SprintController(repo.Object);

            var result = controller.DeleteSprint(id);

            Assert.IsType<NoContentResult>(result);
            repo.Verify(r => r.DeleteSprint(id), Times.Once);
        }

        [Fact]
        public void DeleteSprint_WhenMissing_ReturnsNotFoundAndDoesNotCallDelete()
        {
            var repo = new Mock<ISprintRepository>();
            repo.Setup(r => r.GetSprintById(It.IsAny<Guid>())).Returns((SprintDTO?)null);
            var controller = new SprintController(repo.Object);

            var result = controller.DeleteSprint(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
            repo.Verify(r => r.DeleteSprint(It.IsAny<Guid>()), Times.Never);
        }
    }
}
