using AttachmentService.Controllers;
using AttachmentService.Data;
using AttachmentService.Exceptions;
using AttachmentService.Models.DTO;
using AttachmentService.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AttachmentService.Tests
{
    public class AttachmentControllerTests
    {
        private static AttachmentController CreateController(Mock<IAttachmentRepository> repository, string? userIdHeader = null, string? bearerToken = null)
        {
            var controller = new AttachmentController(repository.Object);
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

        private static AttachmentDTO SampleDto(Guid? id = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
            FileName = "a.txt",
            OriginalFileName = "a.txt",
            ContentType = "text/plain",
            FileSize = 10,
            Status = AttachmentStatus.Ready,
            ProjectId = Guid.NewGuid()
        };

        // ---------- GetAttachments ----------

        [Fact]
        public async Task GetAttachments_ReturnsOkWithRepositoryResult()
        {
            var repo = new Mock<IAttachmentRepository>();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var expected = new List<AttachmentDTO> { SampleDto() };
            repo.Setup(r => r.GetAttachmentsAsync(projectId, null, userId, It.IsAny<string?>())).ReturnsAsync(expected);
            var controller = CreateController(repo, userIdHeader: userId.ToString());

            var result = await controller.GetAttachments(projectId, null);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, ok.Value);
        }

        [Fact]
        public async Task GetAttachments_WithoutUserIdHeader_ReturnsBadRequest()
        {
            var repo = new Mock<IAttachmentRepository>();
            var controller = CreateController(repo, userIdHeader: null);

            var result = await controller.GetAttachments(Guid.NewGuid(), null);

            Assert.IsType<BadRequestObjectResult>(result.Result);
            repo.Verify(r => r.GetAttachmentsAsync(It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task GetAttachments_WhenUserNotAProjectMember_ReturnsForbidden()
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.GetAttachmentsAsync(It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new UserNotProjectMemberException(Guid.NewGuid(), Guid.NewGuid()));
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.GetAttachments(Guid.NewGuid(), null);

            var forbidden = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        }

        [Fact]
        public async Task GetAttachments_WithoutProjectContext_ReturnsBadRequest()
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.GetAttachmentsAsync(null, null, It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new ProjectContextRequiredException(Guid.NewGuid()));
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.GetAttachments(null, null);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAttachmentsForTask_PassesTaskId()
        {
            var repo = new Mock<IAttachmentRepository>();
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            repo.Setup(r => r.GetAttachmentsAsync(null, taskId, userId, It.IsAny<string?>())).ReturnsAsync(new List<AttachmentDTO>());
            var controller = CreateController(repo, userIdHeader: userId.ToString());

            await controller.GetAttachmentsForTask(taskId);

            repo.Verify(r => r.GetAttachmentsAsync(null, taskId, userId, It.IsAny<string?>()), Times.Once);
        }

        // ---------- GetAttachmentById ----------

        [Fact]
        public async Task GetAttachmentById_WhenFound_ReturnsOk()
        {
            var repo = new Mock<IAttachmentRepository>();
            var dto = SampleDto();
            var userId = Guid.NewGuid();
            repo.Setup(r => r.GetAttachmentByIdAsync(dto.Id, userId, It.IsAny<string?>())).ReturnsAsync(dto);
            var controller = CreateController(repo, userIdHeader: userId.ToString());

            var result = await controller.GetAttachmentById(dto.Id);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(dto, ok.Value);
        }

        [Fact]
        public async Task GetAttachmentById_WhenMissing_ReturnsNotFound()
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.GetAttachmentByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>())).ReturnsAsync((AttachmentDTO?)null);
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.GetAttachmentById(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetAttachmentById_WhenUserNotAProjectMember_ReturnsForbidden()
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.GetAttachmentByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new UserNotProjectMemberException(Guid.NewGuid(), Guid.NewGuid()));
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.GetAttachmentById(Guid.NewGuid());

            var forbidden = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        }

        // ---------- DownloadAttachment ----------

        [Fact]
        public async Task DownloadAttachment_WhenUrlAvailable_ReturnsRedirect()
        {
            var repo = new Mock<IAttachmentRepository>();
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            repo.Setup(r => r.GetDownloadUrlAsync(id, userId, It.IsAny<string?>())).ReturnsAsync("https://storage.example/file");
            var controller = CreateController(repo, userIdHeader: userId.ToString());

            var result = await controller.DownloadAttachment(id);

            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal("https://storage.example/file", redirect.Url);
        }

        [Fact]
        public async Task DownloadAttachment_WhenNotReady_ReturnsNotFound()
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.GetDownloadUrlAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>())).ReturnsAsync((string?)null);
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.DownloadAttachment(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DownloadAttachment_WhenUserNotAProjectMember_ReturnsForbidden()
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.GetDownloadUrlAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new UserNotProjectMemberException(Guid.NewGuid(), Guid.NewGuid()));
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.DownloadAttachment(Guid.NewGuid());

            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        }

        // ---------- GetAttachmentDetails ----------

        [Fact]
        public async Task GetAttachmentDetails_WhenFound_ReturnsOk()
        {
            var repo = new Mock<IAttachmentRepository>();
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var details = new AttachmentDetailsDTO { Attachment = SampleDto(id) };
            repo.Setup(r => r.GetAttachmentDetailsAsync(id, userId, It.IsAny<string?>())).ReturnsAsync(details);
            var controller = CreateController(repo, userIdHeader: userId.ToString());

            var result = await controller.GetAttachmentDetails(id);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(details, ok.Value);
        }

        [Fact]
        public async Task GetAttachmentDetails_WhenMissing_ReturnsNotFound()
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.GetAttachmentDetailsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>())).ReturnsAsync((AttachmentDetailsDTO?)null);
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.GetAttachmentDetails(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // ---------- CreateAttachment ----------

        [Fact]
        public async Task CreateAttachment_WithoutUserIdHeader_ReturnsBadRequest()
        {
            var repo = new Mock<IAttachmentRepository>();
            var controller = CreateController(repo, userIdHeader: null);
            var dto = new AttachmentCreationDTO { ProjectId = Guid.NewGuid() };

            var result = await controller.CreateAttachment(dto);

            Assert.IsType<BadRequestObjectResult>(result.Result);
            repo.Verify(r => r.CreateAttachmentAsync(It.IsAny<AttachmentCreationDTO>(), It.IsAny<Guid>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task CreateAttachment_WithInvalidUserIdHeader_ReturnsBadRequest()
        {
            var repo = new Mock<IAttachmentRepository>();
            var controller = CreateController(repo, userIdHeader: "not-a-guid");
            var dto = new AttachmentCreationDTO { ProjectId = Guid.NewGuid() };

            var result = await controller.CreateAttachment(dto);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateAttachment_WithValidUserIdHeader_ReturnsCreatedAtRoute()
        {
            var repo = new Mock<IAttachmentRepository>();
            var userId = Guid.NewGuid();
            var response = new AttachmentUploadResponseDTO { Attachment = SampleDto(), UploadUrl = "https://storage.example/upload" };
            repo.Setup(r => r.CreateAttachmentAsync(It.IsAny<AttachmentCreationDTO>(), userId, It.IsAny<string?>())).ReturnsAsync(response);
            var controller = CreateController(repo, userIdHeader: userId.ToString());
            var dto = new AttachmentCreationDTO { ProjectId = Guid.NewGuid() };

            var result = await controller.CreateAttachment(dto);

            var created = Assert.IsType<CreatedAtRouteResult>(result.Result);
            Assert.Equal("GetAttachmentById", created.RouteName);
            Assert.Equal(response.Attachment.Id, created.RouteValues!["id"]);
            Assert.Same(response, created.Value);
        }

        [Fact]
        public async Task CreateAttachment_WhenProjectDoesNotExist_ReturnsBadRequest()
        {
            var repo = new Mock<IAttachmentRepository>();
            var projectId = Guid.NewGuid();
            repo.Setup(r => r.CreateAttachmentAsync(It.IsAny<AttachmentCreationDTO>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new ProjectNotFoundException(projectId));
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.CreateAttachment(new AttachmentCreationDTO { ProjectId = projectId });

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains(projectId.ToString(), badRequest.Value!.ToString());
        }

        [Fact]
        public async Task CreateAttachment_WhenTaskDoesNotExist_ReturnsBadRequest()
        {
            var repo = new Mock<IAttachmentRepository>();
            var taskId = Guid.NewGuid();
            repo.Setup(r => r.CreateAttachmentAsync(It.IsAny<AttachmentCreationDTO>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new TaskNotFoundException(taskId));
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.CreateAttachment(new AttachmentCreationDTO { ProjectId = Guid.NewGuid(), TaskId = taskId });

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateAttachment_WhenUserNotAProjectMember_ReturnsForbidden()
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.CreateAttachmentAsync(It.IsAny<AttachmentCreationDTO>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new UserNotProjectMemberException(Guid.NewGuid(), Guid.NewGuid()));
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.CreateAttachment(new AttachmentCreationDTO { ProjectId = Guid.NewGuid() });

            var forbidden = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        }

        [Fact]
        public async Task CreateAttachment_WhenRoleCannotUpload_ReturnsForbidden()
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.CreateAttachmentAsync(It.IsAny<AttachmentCreationDTO>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new RoleCannotUploadAttachmentsException(Guid.NewGuid(), "Client"));
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.CreateAttachment(new AttachmentCreationDTO { ProjectId = Guid.NewGuid() });

            var forbidden = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        }

        // ---------- CreateAttachmentForTask ----------

        [Fact]
        public async Task CreateAttachmentForTask_OverridesTaskIdFromRoute()
        {
            var repo = new Mock<IAttachmentRepository>();
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var response = new AttachmentUploadResponseDTO { Attachment = SampleDto(), UploadUrl = "url" };
            repo.Setup(r => r.CreateAttachmentAsync(It.Is<AttachmentCreationDTO>(d => d.TaskId == taskId), userId, It.IsAny<string?>()))
                .ReturnsAsync(response);
            var controller = CreateController(repo, userIdHeader: userId.ToString());
            // Body doesn't mention a TaskId at all - the route should still win.
            var dto = new AttachmentCreationDTO { ProjectId = Guid.NewGuid() };

            var result = await controller.CreateAttachmentForTask(taskId, dto);

            Assert.IsType<CreatedAtRouteResult>(result.Result);
            repo.Verify(r => r.CreateAttachmentAsync(It.Is<AttachmentCreationDTO>(d => d.TaskId == taskId), userId, It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task CreateAttachmentForTask_WithoutUserIdHeader_ReturnsBadRequest()
        {
            var repo = new Mock<IAttachmentRepository>();
            var controller = CreateController(repo, userIdHeader: null);
            var dto = new AttachmentCreationDTO { ProjectId = Guid.NewGuid() };

            var result = await controller.CreateAttachmentForTask(Guid.NewGuid(), dto);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        // ---------- ConfirmAttachment ----------

        [Theory]
        [InlineData(ConfirmAttachmentOutcome.NotFound, typeof(NotFoundResult))]
        [InlineData(ConfirmAttachmentOutcome.Forbidden, typeof(ObjectResult))]
        [InlineData(ConfirmAttachmentOutcome.InvalidState, typeof(ConflictObjectResult))]
        [InlineData(ConfirmAttachmentOutcome.ObjectMissing, typeof(ConflictObjectResult))]
        public async Task ConfirmAttachment_MapsEachFailureOutcomeToExpectedStatusCode(ConfirmAttachmentOutcome outcome, Type expectedResultType)
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.ConfirmAttachmentAsync(It.IsAny<AttachmentConfirmationDTO>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ReturnsAsync(new ConfirmAttachmentResult(outcome, null));
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.ConfirmAttachment(new AttachmentConfirmationDTO { AttachmentId = Guid.NewGuid() });

            Assert.IsType(expectedResultType, result.Result);
        }

        [Fact]
        public async Task ConfirmAttachment_OnSuccess_ReturnsOkWithAttachment()
        {
            var repo = new Mock<IAttachmentRepository>();
            var dto = SampleDto();
            repo.Setup(r => r.ConfirmAttachmentAsync(It.IsAny<AttachmentConfirmationDTO>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ReturnsAsync(new ConfirmAttachmentResult(ConfirmAttachmentOutcome.Success, dto));
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.ConfirmAttachment(new AttachmentConfirmationDTO { AttachmentId = dto.Id });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(dto, ok.Value);
        }

        [Fact]
        public async Task ConfirmAttachment_WithoutUserIdHeader_ReturnsBadRequest()
        {
            var repo = new Mock<IAttachmentRepository>();
            var controller = CreateController(repo, userIdHeader: null);

            var result = await controller.ConfirmAttachment(new AttachmentConfirmationDTO { AttachmentId = Guid.NewGuid() });

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        // ---------- UpdateAttachment ----------

        [Fact]
        public async Task UpdateAttachment_WhenFound_ReturnsOk()
        {
            var repo = new Mock<IAttachmentRepository>();
            var dto = SampleDto();
            var userId = Guid.NewGuid();
            repo.Setup(r => r.UpdateAttachmentAsync(dto.Id, It.IsAny<AttachmentUpdateDTO>(), userId, It.IsAny<string?>())).ReturnsAsync(dto);
            var controller = CreateController(repo, userIdHeader: userId.ToString());

            var result = await controller.UpdateAttachment(dto.Id, new AttachmentUpdateDTO { Description = "x" });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(dto, ok.Value);
        }

        [Fact]
        public async Task UpdateAttachment_WhenMissing_ReturnsNotFound()
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.UpdateAttachmentAsync(It.IsAny<Guid>(), It.IsAny<AttachmentUpdateDTO>(), It.IsAny<Guid>(), It.IsAny<string?>())).ReturnsAsync((AttachmentDTO?)null);
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.UpdateAttachment(Guid.NewGuid(), new AttachmentUpdateDTO());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdateAttachment_WithoutUserIdHeader_ReturnsBadRequest()
        {
            var repo = new Mock<IAttachmentRepository>();
            var controller = CreateController(repo, userIdHeader: null);

            var result = await controller.UpdateAttachment(Guid.NewGuid(), new AttachmentUpdateDTO());

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateAttachment_WhenActingUserIsNotOwner_ReturnsForbidden()
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.UpdateAttachmentAsync(It.IsAny<Guid>(), It.IsAny<AttachmentUpdateDTO>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new NotAttachmentOwnerException(Guid.NewGuid(), Guid.NewGuid()));
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.UpdateAttachment(Guid.NewGuid(), new AttachmentUpdateDTO { Description = "x" });

            var forbidden = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        }

        // ---------- DeleteAttachment ----------

        [Fact]
        public async Task DeleteAttachment_WhenFound_ReturnsNoContentAndCallsRepository()
        {
            var repo = new Mock<IAttachmentRepository>();
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            repo.Setup(r => r.DeleteAttachmentAsync(id, userId, It.IsAny<string?>())).ReturnsAsync(true);
            var controller = CreateController(repo, userIdHeader: userId.ToString());

            var result = await controller.DeleteAttachment(id);

            Assert.IsType<NoContentResult>(result);
            repo.Verify(r => r.DeleteAttachmentAsync(id, userId, It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAttachment_WhenMissing_ReturnsNotFound()
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.DeleteAttachmentAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>())).ReturnsAsync(false);
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.DeleteAttachment(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteAttachment_WithoutUserIdHeader_ReturnsBadRequest()
        {
            var repo = new Mock<IAttachmentRepository>();
            var controller = CreateController(repo, userIdHeader: null);

            var result = await controller.DeleteAttachment(Guid.NewGuid());

            Assert.IsType<BadRequestObjectResult>(result);
            repo.Verify(r => r.DeleteAttachmentAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAttachment_WhenActingUserIsNotOwner_ReturnsForbidden()
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.DeleteAttachmentAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new NotAttachmentOwnerException(Guid.NewGuid(), Guid.NewGuid()));
            var controller = CreateController(repo, userIdHeader: Guid.NewGuid().ToString());

            var result = await controller.DeleteAttachment(Guid.NewGuid());

            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        }
    }
}
