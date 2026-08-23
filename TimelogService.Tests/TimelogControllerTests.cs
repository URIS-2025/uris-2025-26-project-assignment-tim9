using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TimelogService.Controllers;
using TimelogService.Data;
using TimelogService.Exceptions;
using TimelogService.Models.DTO;

namespace TimelogService.Tests
{
    public class TimelogControllerTests
    {
        private static TimelogController CreateController(Mock<ITimelogRepository> repository, string? userIdHeader = null, string? bearerToken = null)
        {
            var controller = new TimelogController(repository.Object);
            var httpContext = new DefaultHttpContext();

            if (userIdHeader is not null)
            {
                httpContext.Request.Headers["X-User-Id"] = userIdHeader;
            }

            if (bearerToken is not null)
            {
                httpContext.Request.Headers.Authorization = $"Bearer {bearerToken}";
            }

            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        private static TimelogDTO SampleDto(Guid? id = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
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
            var taskId = Guid.NewGuid();
            repo.Setup(r => r.GetTimelogs(projectId, taskId)).Returns(new List<TimelogDTO>());
            var controller = CreateController(repo);

            controller.GetTimelogs(projectId, taskId);

            repo.Verify(r => r.GetTimelogs(projectId, taskId), Times.Once);
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
            var dto = new TimelogCreationDTO { ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid() };

            var result = await controller.CreateTimelog(dto);

            Assert.IsType<BadRequestObjectResult>(result.Result);
            repo.Verify(r => r.CreateTimelogAsync(It.IsAny<TimelogCreationDTO>(), It.IsAny<Guid>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task CreateTimelog_WithInvalidUserIdHeader_ReturnsBadRequest()
        {
            var repo = new Mock<ITimelogRepository>();
            var controller = CreateController(repo, userIdHeader: "not-a-guid");
            var dto = new TimelogCreationDTO { ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid() };

            var result = await controller.CreateTimelog(dto);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateTimelog_WithValidUserIdHeader_ReturnsCreatedAtRoute()
        {
            var repo = new Mock<ITimelogRepository>();
            var userId = Guid.NewGuid();
            var confirmation = new TimelogConfirmationDTO { Id = Guid.NewGuid() };
            repo.Setup(r => r.CreateTimelogAsync(It.IsAny<TimelogCreationDTO>(), userId, It.IsAny<string?>())).ReturnsAsync(confirmation);
            var controller = CreateController(repo, userIdHeader: userId.ToString());
            var dto = new TimelogCreationDTO { ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid() };

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
            repo.Setup(r => r.CreateTimelogAsync(It.IsAny<TimelogCreationDTO>(), userId, It.IsAny<string?>()))
                .ReturnsAsync(new TimelogConfirmationDTO { Id = Guid.NewGuid() });
            var controller = CreateController(repo, userIdHeader: userId.ToString());

            await controller.CreateTimelog(new TimelogCreationDTO { ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid() });

            repo.Verify(r => r.CreateTimelogAsync(It.IsAny<TimelogCreationDTO>(), userId, It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task CreateTimelog_PassesBearerTokenFromAuthorizationHeaderToRepository()
        {
            var repo = new Mock<ITimelogRepository>();
            var userId = Guid.NewGuid();
            repo.Setup(r => r.CreateTimelogAsync(It.IsAny<TimelogCreationDTO>(), userId, "my-token"))
                .ReturnsAsync(new TimelogConfirmationDTO { Id = Guid.NewGuid() });
            var controller = CreateController(repo, userIdHeader: userId.ToString(), bearerToken: "my-token");

            await controller.CreateTimelog(new TimelogCreationDTO { ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid() });

            repo.Verify(r => r.CreateTimelogAsync(It.IsAny<TimelogCreationDTO>(), userId, "my-token"), Times.Once);
        }

        [Fact]
        public async Task CreateTimelog_WhenTaskDoesNotExist_ReturnsBadRequest()
        {
            var repo = new Mock<ITimelogRepository>();
            var taskId = Guid.NewGuid();
            repo.Setup(r => r.CreateTimelogAsync(It.IsAny<TimelogCreationDTO>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new TaskNotFoundException(taskId));
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());
            var dto = new TimelogCreationDTO { ProjectId = Guid.NewGuid(), TaskId = taskId };

            var result = await controller.CreateTimelog(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains(taskId.ToString(), badRequest.Value!.ToString());
        }

        [Fact]
        public async Task CreateTimelog_WhenProjectDoesNotExist_ReturnsBadRequest()
        {
            var repo = new Mock<ITimelogRepository>();
            var projectId = Guid.NewGuid();
            repo.Setup(r => r.CreateTimelogAsync(It.IsAny<TimelogCreationDTO>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new ProjectNotFoundException(projectId));
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());
            var dto = new TimelogCreationDTO { ProjectId = projectId, TaskId = Guid.NewGuid() };

            var result = await controller.CreateTimelog(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains(projectId.ToString(), badRequest.Value!.ToString());
        }

        [Fact]
        public async Task CreateTimelog_WhenUserIsNotAProjectMember_ReturnsForbidden()
        {
            var repo = new Mock<ITimelogRepository>();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            repo.Setup(r => r.CreateTimelogAsync(It.IsAny<TimelogCreationDTO>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new UserNotProjectMemberException(userId, projectId));
            var controller = CreateController(repo, userIdHeader: userId.ToString());
            var dto = new TimelogCreationDTO { ProjectId = projectId, TaskId = Guid.NewGuid() };

            var result = await controller.CreateTimelog(dto);

            var forbidden = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        }

        [Fact]
        public async Task CreateTimelog_WhenUserHasClientRole_ReturnsForbidden()
        {
            var repo = new Mock<ITimelogRepository>();
            var userId = Guid.NewGuid();
            repo.Setup(r => r.CreateTimelogAsync(It.IsAny<TimelogCreationDTO>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new ClientCannotLogTimeException(userId));
            var controller = CreateController(repo, userIdHeader: userId.ToString());
            var dto = new TimelogCreationDTO { ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid() };

            var result = await controller.CreateTimelog(dto);

            var forbidden = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        }

        // ---------- UpdateTimelog ----------

        [Fact]
        public async Task UpdateTimelog_WhenFound_ReturnsOk()
        {
            var repo = new Mock<ITimelogRepository>();
            var id = Guid.NewGuid();
            var confirmation = new TimelogConfirmationDTO { Id = id };
            repo.Setup(r => r.UpdateTimelogAsync(id, It.IsAny<TimelogUpdateDTO>(), It.IsAny<Guid>(), It.IsAny<string?>())).ReturnsAsync(confirmation);
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.UpdateTimelog(id, new TimelogUpdateDTO { HoursSpent = 5 });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(confirmation, ok.Value);
        }

        [Fact]
        public async Task UpdateTimelog_WithoutUserIdHeader_ReturnsBadRequest()
        {
            var repo = new Mock<ITimelogRepository>();
            var controller = CreateController(repo, userIdHeader: null);

            var result = await controller.UpdateTimelog(Guid.NewGuid(), new TimelogUpdateDTO { HoursSpent = 5 });

            Assert.IsType<BadRequestObjectResult>(result.Result);
            repo.Verify(r => r.UpdateTimelogAsync(It.IsAny<Guid>(), It.IsAny<TimelogUpdateDTO>(), It.IsAny<Guid>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTimelog_WhenMissing_ReturnsNotFound()
        {
            var repo = new Mock<ITimelogRepository>();
            repo.Setup(r => r.UpdateTimelogAsync(It.IsAny<Guid>(), It.IsAny<TimelogUpdateDTO>(), It.IsAny<Guid>(), It.IsAny<string?>())).ReturnsAsync((TimelogConfirmationDTO?)null);
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.UpdateTimelog(Guid.NewGuid(), new TimelogUpdateDTO());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdateTimelog_WhenNewTaskDoesNotExist_ReturnsBadRequest()
        {
            var repo = new Mock<ITimelogRepository>();
            var taskId = Guid.NewGuid();
            repo.Setup(r => r.UpdateTimelogAsync(It.IsAny<Guid>(), It.IsAny<TimelogUpdateDTO>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new TaskNotFoundException(taskId));
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.UpdateTimelog(Guid.NewGuid(), new TimelogUpdateDTO { TaskId = taskId });

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateTimelog_WhenLoggerIsNotAMemberOfTheProject_ReturnsForbidden()
        {
            var repo = new Mock<ITimelogRepository>();
            repo.Setup(r => r.UpdateTimelogAsync(It.IsAny<Guid>(), It.IsAny<TimelogUpdateDTO>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new UserNotProjectMemberException(Guid.NewGuid(), Guid.NewGuid()));
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.UpdateTimelog(Guid.NewGuid(), new TimelogUpdateDTO { ProjectId = Guid.NewGuid() });

            var forbidden = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        }

        [Fact]
        public async Task UpdateTimelog_WhenLoggerHasClientRole_ReturnsForbidden()
        {
            var repo = new Mock<ITimelogRepository>();
            repo.Setup(r => r.UpdateTimelogAsync(It.IsAny<Guid>(), It.IsAny<TimelogUpdateDTO>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new ClientCannotLogTimeException(Guid.NewGuid()));
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.UpdateTimelog(Guid.NewGuid(), new TimelogUpdateDTO());

            var forbidden = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        }

        [Fact]
        public async Task UpdateTimelog_WhenActingUserIsNotOwner_ReturnsForbidden()
        {
            var repo = new Mock<ITimelogRepository>();
            repo.Setup(r => r.UpdateTimelogAsync(It.IsAny<Guid>(), It.IsAny<TimelogUpdateDTO>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new NotTimelogOwnerException(Guid.NewGuid(), Guid.NewGuid()));
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.UpdateTimelog(Guid.NewGuid(), new TimelogUpdateDTO { HoursSpent = 5 });

            var forbidden = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        }

        [Fact]
        public async Task UpdateTimelog_PassesActingUserIdFromHeaderToRepository()
        {
            var repo = new Mock<ITimelogRepository>();
            var actingUserId = Guid.NewGuid();
            var id = Guid.NewGuid();
            repo.Setup(r => r.UpdateTimelogAsync(id, It.IsAny<TimelogUpdateDTO>(), actingUserId, It.IsAny<string?>()))
                .ReturnsAsync(new TimelogConfirmationDTO { Id = id });
            var controller = CreateController(repo, userIdHeader: actingUserId.ToString());

            await controller.UpdateTimelog(id, new TimelogUpdateDTO { HoursSpent = 5 });

            repo.Verify(r => r.UpdateTimelogAsync(id, It.IsAny<TimelogUpdateDTO>(), actingUserId, It.IsAny<string?>()), Times.Once);
        }

        // ---------- DeleteTimelog ----------

        [Fact]
        public async Task DeleteTimelog_WhenFound_ReturnsNoContentAndCallsRepository()
        {
            var repo = new Mock<ITimelogRepository>();
            var id = Guid.NewGuid();
            var actingUserId = Guid.NewGuid();
            repo.Setup(r => r.GetTimelogById(id)).Returns(SampleDto(id));
            var controller = CreateController(repo, userIdHeader: actingUserId.ToString());

            var result = await controller.DeleteTimelog(id);

            Assert.IsType<NoContentResult>(result);
            repo.Verify(r => r.DeleteTimelogAsync(id, actingUserId, It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task DeleteTimelog_WhenMissing_ReturnsNotFoundAndDoesNotCallDelete()
        {
            var repo = new Mock<ITimelogRepository>();
            repo.Setup(r => r.GetTimelogById(It.IsAny<Guid>())).Returns((TimelogDTO?)null);
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.DeleteTimelog(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
            repo.Verify(r => r.DeleteTimelogAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task DeleteTimelog_WithoutUserIdHeader_ReturnsBadRequest()
        {
            var repo = new Mock<ITimelogRepository>();
            var controller = CreateController(repo, userIdHeader: null);

            var result = await controller.DeleteTimelog(Guid.NewGuid());

            Assert.IsType<BadRequestObjectResult>(result);
            repo.Verify(r => r.DeleteTimelogAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task DeleteTimelog_WhenActingUserIsNotOwner_ReturnsForbidden()
        {
            var repo = new Mock<ITimelogRepository>();
            var id = Guid.NewGuid();
            repo.Setup(r => r.GetTimelogById(id)).Returns(SampleDto(id));
            repo.Setup(r => r.DeleteTimelogAsync(id, It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new NotTimelogOwnerException(Guid.NewGuid(), Guid.NewGuid()));
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.DeleteTimelog(id);

            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        }
    }
}
