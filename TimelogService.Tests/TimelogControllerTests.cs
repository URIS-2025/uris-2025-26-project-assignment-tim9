using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TimelogService.Controllers;
using TimelogService.Data;
using TimelogService.Models.DTO;

namespace TimelogService.Tests
{
    public class TimelogControllerTests
    {
        private static TimelogController CreateController(Mock<ITimelogRepository> repository, string? userIdHeader = null)
        {
            var controller = new TimelogController(repository.Object);
            var httpContext = new DefaultHttpContext();

            if (userIdHeader is not null)
            {
                httpContext.Request.Headers["X-User-Id"] = userIdHeader;
            }

            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        private static TimelogDTO SampleDto(Guid? id = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            WorkPackageId = Guid.NewGuid(),
            HoursSpent = 3,
            Date = new DateTime(2026, 1, 1)
        };

        // ---------- GetTimelogs ----------

        [Fact]
        public void GetTimelogs_ReturnsOkWithRepositoryResult()
        {
            var repo = new Mock<ITimelogRepository>();
            var expected = new List<TimelogDTO> { SampleDto() };
            repo.Setup(r => r.GetTimelogs(null, null)).Returns(expected);
            var controller = CreateController(repo);

            var result = controller.GetTimelogs(null, null);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, ok.Value);
        }

        [Fact]
        public void GetTimelogs_WithEmptyResult_StillReturnsOkNotNoContent()
        {
            var repo = new Mock<ITimelogRepository>();
            repo.Setup(r => r.GetTimelogs(null, null)).Returns(new List<TimelogDTO>());
            var controller = CreateController(repo);

            var result = controller.GetTimelogs(null, null);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Empty((IEnumerable<TimelogDTO>)ok.Value!);
        }

        [Fact]
        public void GetTimelogs_PassesBothFiltersThrough()
        {
            var repo = new Mock<ITimelogRepository>();
            var projectId = Guid.NewGuid();
            var workPackageId = Guid.NewGuid();
            repo.Setup(r => r.GetTimelogs(projectId, workPackageId)).Returns(new List<TimelogDTO>());
            var controller = CreateController(repo);

            controller.GetTimelogs(projectId, workPackageId);

            repo.Verify(r => r.GetTimelogs(projectId, workPackageId), Times.Once);
        }

        // ---------- GetTimelogById ----------

        [Fact]
        public void GetTimelogById_WhenFound_ReturnsOk()
        {
            var repo = new Mock<ITimelogRepository>();
            var dto = SampleDto();
            repo.Setup(r => r.GetTimelogById(dto.Id)).Returns(dto);
            var controller = CreateController(repo);

            var result = controller.GetTimelogById(dto.Id);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(dto, ok.Value);
        }

        [Fact]
        public void GetTimelogById_WhenMissing_ReturnsNotFound()
        {
            var repo = new Mock<ITimelogRepository>();
            repo.Setup(r => r.GetTimelogById(It.IsAny<Guid>())).Returns((TimelogDTO?)null);
            var controller = CreateController(repo);

            var result = controller.GetTimelogById(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // ---------- CreateTimelog ----------

        [Fact]
        public async Task CreateTimelog_WithoutUserIdHeader_ReturnsBadRequest()
        {
            var repo = new Mock<ITimelogRepository>();
            var controller = CreateController(repo, userIdHeader: null);
            var dto = new TimelogCreationDTO { ProjectId = Guid.NewGuid(), WorkPackageId = Guid.NewGuid() };

            var result = await controller.CreateTimelog(dto);

            Assert.IsType<BadRequestObjectResult>(result.Result);
            repo.Verify(r => r.CreateTimelogAsync(It.IsAny<TimelogCreationDTO>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task CreateTimelog_WithInvalidUserIdHeader_ReturnsBadRequest()
        {
            var repo = new Mock<ITimelogRepository>();
            var controller = CreateController(repo, userIdHeader: "not-a-guid");
            var dto = new TimelogCreationDTO { ProjectId = Guid.NewGuid(), WorkPackageId = Guid.NewGuid() };

            var result = await controller.CreateTimelog(dto);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateTimelog_WithValidUserIdHeader_ReturnsCreatedAtRoute()
        {
            var repo = new Mock<ITimelogRepository>();
            var userId = Guid.NewGuid();
            var confirmation = new TimelogConfirmationDTO { Id = Guid.NewGuid() };
            repo.Setup(r => r.CreateTimelogAsync(It.IsAny<TimelogCreationDTO>(), userId)).ReturnsAsync(confirmation);
            var controller = CreateController(repo, userIdHeader: userId.ToString());
            var dto = new TimelogCreationDTO { ProjectId = Guid.NewGuid(), WorkPackageId = Guid.NewGuid() };

            var result = await controller.CreateTimelog(dto);

            var created = Assert.IsType<CreatedAtRouteResult>(result.Result);
            Assert.Equal("GetTimelogById", created.RouteName);
            Assert.Equal(confirmation.Id, created.RouteValues!["id"]);
            Assert.Same(confirmation, created.Value);
        }

        [Fact]
        public async Task CreateTimelog_PassesUserIdFromHeaderToRepository()
        {
            var repo = new Mock<ITimelogRepository>();
            var userId = Guid.NewGuid();
            repo.Setup(r => r.CreateTimelogAsync(It.IsAny<TimelogCreationDTO>(), userId))
                .ReturnsAsync(new TimelogConfirmationDTO { Id = Guid.NewGuid() });
            var controller = CreateController(repo, userIdHeader: userId.ToString());

            await controller.CreateTimelog(new TimelogCreationDTO { ProjectId = Guid.NewGuid(), WorkPackageId = Guid.NewGuid() });

            repo.Verify(r => r.CreateTimelogAsync(It.IsAny<TimelogCreationDTO>(), userId), Times.Once);
        }

        // ---------- UpdateTimelog ----------

        [Fact]
        public async Task UpdateTimelog_WhenFound_ReturnsOk()
        {
            var repo = new Mock<ITimelogRepository>();
            var id = Guid.NewGuid();
            var confirmation = new TimelogConfirmationDTO { Id = id };
            repo.Setup(r => r.UpdateTimelogAsync(id, It.IsAny<TimelogUpdateDTO>())).ReturnsAsync(confirmation);
            var controller = CreateController(repo);

            var result = await controller.UpdateTimelog(id, new TimelogUpdateDTO { HoursSpent = 5 });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(confirmation, ok.Value);
        }

        [Fact]
        public async Task UpdateTimelog_WhenMissing_ReturnsNotFound()
        {
            var repo = new Mock<ITimelogRepository>();
            repo.Setup(r => r.UpdateTimelogAsync(It.IsAny<Guid>(), It.IsAny<TimelogUpdateDTO>())).ReturnsAsync((TimelogConfirmationDTO?)null);
            var controller = CreateController(repo);

            var result = await controller.UpdateTimelog(Guid.NewGuid(), new TimelogUpdateDTO());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // ---------- DeleteTimelog ----------

        [Fact]
        public void DeleteTimelog_WhenFound_ReturnsNoContentAndCallsRepository()
        {
            var repo = new Mock<ITimelogRepository>();
            var id = Guid.NewGuid();
            repo.Setup(r => r.GetTimelogById(id)).Returns(SampleDto(id));
            var controller = CreateController(repo);

            var result = controller.DeleteTimelog(id);

            Assert.IsType<NoContentResult>(result);
            repo.Verify(r => r.DeleteTimelog(id), Times.Once);
        }

        [Fact]
        public void DeleteTimelog_WhenMissing_ReturnsNotFoundAndDoesNotCallDelete()
        {
            var repo = new Mock<ITimelogRepository>();
            repo.Setup(r => r.GetTimelogById(It.IsAny<Guid>())).Returns((TimelogDTO?)null);
            var controller = CreateController(repo);

            var result = controller.DeleteTimelog(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
            repo.Verify(r => r.DeleteTimelog(It.IsAny<Guid>()), Times.Never);
        }
    }
}
