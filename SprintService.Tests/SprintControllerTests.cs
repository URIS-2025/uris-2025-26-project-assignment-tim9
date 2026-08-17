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

        // ---------- GetSprints ----------

        [Fact]
        public void GetSprints_ReturnsOkWithRepositoryResult()
        {
            var repo = new Mock<ISprintRepository>();
            var expected = new List<SprintDTO> { SampleDto() };
            repo.Setup(r => r.GetSprints(null)).Returns(expected);
            var controller = new SprintController(repo.Object);

            var result = controller.GetSprints(null);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, ok.Value);
        }

        [Fact]
        public void GetSprints_WithEmptyResult_StillReturnsOkNotNoContent()
        {
            // Reproduces a real fix: this used to return 204 for an empty list, which has no
            // body and can break strongly-typed clients expecting to deserialize a JSON array.
            var repo = new Mock<ISprintRepository>();
            repo.Setup(r => r.GetSprints(null)).Returns(new List<SprintDTO>());
            var controller = new SprintController(repo.Object);

            var result = controller.GetSprints(null);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Empty((IEnumerable<SprintDTO>)ok.Value!);
        }

        [Fact]
        public void GetSprints_PassesProjectIdFilterThrough()
        {
            var repo = new Mock<ISprintRepository>();
            var projectId = Guid.NewGuid();
            repo.Setup(r => r.GetSprints(projectId)).Returns(new List<SprintDTO>());
            var controller = new SprintController(repo.Object);

            controller.GetSprints(projectId);

            repo.Verify(r => r.GetSprints(projectId), Times.Once);
        }

        // ---------- GetSprintsForProject (required route: GET /projects/{projectId}/sprints) ----------

        [Fact]
        public void GetSprintsForProject_PassesProjectIdThrough()
        {
            var repo = new Mock<ISprintRepository>();
            var projectId = Guid.NewGuid();
            var expected = new List<SprintDTO> { SampleDto() };
            repo.Setup(r => r.GetSprints(projectId)).Returns(expected);
            var controller = new SprintController(repo.Object);

            var result = controller.GetSprintsForProject(projectId);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, ok.Value);
        }

        // ---------- GetSprintById ----------

        [Fact]
        public void GetSprintById_WhenFound_ReturnsOk()
        {
            var repo = new Mock<ISprintRepository>();
            var dto = SampleDto();
            repo.Setup(r => r.GetSprintById(dto.Id)).Returns(dto);
            var controller = new SprintController(repo.Object);

            var result = controller.GetSprintById(dto.Id);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(dto, ok.Value);
        }

        [Fact]
        public void GetSprintById_WhenMissing_ReturnsNotFound()
        {
            var repo = new Mock<ISprintRepository>();
            repo.Setup(r => r.GetSprintById(It.IsAny<Guid>())).Returns((SprintDTO?)null);
            var controller = new SprintController(repo.Object);

            var result = controller.GetSprintById(Guid.NewGuid());

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
