using AttachmentService.Controllers;
using AttachmentService.Data;
using AttachmentService.Models.DTO;
using AttachmentService.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AttachmentService.Tests
{
    public class AttachmentControllerTests
    {
        private static AttachmentController CreateController(Mock<IAttachmentRepository> repository, string? userIdHeader = null)
        {
            var controller = new AttachmentController(repository.Object);
            var httpContext = new DefaultHttpContext();

            if (userIdHeader is not null)
            {
                httpContext.Request.Headers["X-User-Id"] = userIdHeader;
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
        public void GetAttachments_ReturnsOkWithRepositoryResult()
        {
            var repo = new Mock<IAttachmentRepository>();
            var projectId = Guid.NewGuid();
            var expected = new List<AttachmentDTO> { SampleDto() };
            repo.Setup(r => r.GetAttachments(projectId, null)).Returns(expected);
            var controller = CreateController(repo);

            var result = controller.GetAttachments(projectId, null);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, ok.Value);
        }

        [Fact]
        public void GetAttachmentsForTask_PassesTaskIdAsWorkPackageId()
        {
            var repo = new Mock<IAttachmentRepository>();
            var taskId = Guid.NewGuid();
            repo.Setup(r => r.GetAttachments(null, taskId)).Returns(new List<AttachmentDTO>());
            var controller = CreateController(repo);

            controller.GetAttachmentsForTask(taskId);

            repo.Verify(r => r.GetAttachments(null, taskId), Times.Once);
        }

        // ---------- GetAttachmentById ----------

        [Fact]
        public void GetAttachmentById_WhenFound_ReturnsOk()
        {
            var repo = new Mock<IAttachmentRepository>();
            var dto = SampleDto();
            repo.Setup(r => r.GetAttachmentById(dto.Id)).Returns(dto);
            var controller = CreateController(repo);

            var result = controller.GetAttachmentById(dto.Id);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(dto, ok.Value);
        }

        [Fact]
        public void GetAttachmentById_WhenMissing_ReturnsNotFound()
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.GetAttachmentById(It.IsAny<Guid>())).Returns((AttachmentDTO?)null);
            var controller = CreateController(repo);

            var result = controller.GetAttachmentById(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // ---------- DownloadAttachment ----------

        [Fact]
        public void DownloadAttachment_WhenUrlAvailable_ReturnsRedirect()
        {
            var repo = new Mock<IAttachmentRepository>();
            var id = Guid.NewGuid();
            repo.Setup(r => r.GetDownloadUrl(id)).Returns("https://storage.example/file");
            var controller = CreateController(repo);

            var result = controller.DownloadAttachment(id);

            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal("https://storage.example/file", redirect.Url);
        }

        [Fact]
        public void DownloadAttachment_WhenNotReady_ReturnsNotFound()
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.GetDownloadUrl(It.IsAny<Guid>())).Returns((string?)null);
            var controller = CreateController(repo);

            var result = controller.DownloadAttachment(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        // ---------- GetAttachmentDetails ----------

        [Fact]
        public async Task GetAttachmentDetails_WhenFound_ReturnsOk()
        {
            var repo = new Mock<IAttachmentRepository>();
            var id = Guid.NewGuid();
            var details = new AttachmentDetailsDTO { Attachment = SampleDto(id) };
            repo.Setup(r => r.GetAttachmentDetailsAsync(id)).ReturnsAsync(details);
            var controller = CreateController(repo);

            var result = await controller.GetAttachmentDetails(id);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(details, ok.Value);
        }

        [Fact]
        public async Task GetAttachmentDetails_WhenMissing_ReturnsNotFound()
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.GetAttachmentDetailsAsync(It.IsAny<Guid>())).ReturnsAsync((AttachmentDetailsDTO?)null);
            var controller = CreateController(repo);

            var result = await controller.GetAttachmentDetails(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // ---------- CreateAttachment ----------

        [Fact]
        public void CreateAttachment_WithoutUserIdHeader_ReturnsBadRequest()
        {
            var repo = new Mock<IAttachmentRepository>();
            var controller = CreateController(repo, userIdHeader: null);
            var dto = new AttachmentCreationDTO { ProjectId = Guid.NewGuid() };

            var result = controller.CreateAttachment(dto);

            Assert.IsType<BadRequestObjectResult>(result.Result);
            repo.Verify(r => r.CreateAttachment(It.IsAny<AttachmentCreationDTO>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public void CreateAttachment_WithInvalidUserIdHeader_ReturnsBadRequest()
        {
            var repo = new Mock<IAttachmentRepository>();
            var controller = CreateController(repo, userIdHeader: "not-a-guid");
            var dto = new AttachmentCreationDTO { ProjectId = Guid.NewGuid() };

            var result = controller.CreateAttachment(dto);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void CreateAttachment_WithValidUserIdHeader_ReturnsCreatedAtRoute()
        {
            var repo = new Mock<IAttachmentRepository>();
            var userId = Guid.NewGuid();
            var response = new AttachmentUploadResponseDTO { Attachment = SampleDto(), UploadUrl = "https://storage.example/upload" };
            repo.Setup(r => r.CreateAttachment(It.IsAny<AttachmentCreationDTO>(), userId)).Returns(response);
            var controller = CreateController(repo, userIdHeader: userId.ToString());
            var dto = new AttachmentCreationDTO { ProjectId = Guid.NewGuid() };

            var result = controller.CreateAttachment(dto);

            var created = Assert.IsType<CreatedAtRouteResult>(result.Result);
            Assert.Equal("GetAttachmentById", created.RouteName);
            Assert.Equal(response.Attachment.Id, created.RouteValues!["id"]);
            Assert.Same(response, created.Value);
        }

        // ---------- CreateAttachmentForTask ----------

        [Fact]
        public void CreateAttachmentForTask_OverridesWorkPackageIdFromRoute()
        {
            var repo = new Mock<IAttachmentRepository>();
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var response = new AttachmentUploadResponseDTO { Attachment = SampleDto(), UploadUrl = "url" };
            repo.Setup(r => r.CreateAttachment(It.Is<AttachmentCreationDTO>(d => d.WorkPackageId == taskId), userId))
                .Returns(response);
            var controller = CreateController(repo, userIdHeader: userId.ToString());
            // Body doesn't mention a WorkPackageId at all - the route should still win.
            var dto = new AttachmentCreationDTO { ProjectId = Guid.NewGuid() };

            var result = controller.CreateAttachmentForTask(taskId, dto);

            Assert.IsType<CreatedAtRouteResult>(result.Result);
            repo.Verify(r => r.CreateAttachment(It.Is<AttachmentCreationDTO>(d => d.WorkPackageId == taskId), userId), Times.Once);
        }

        [Fact]
        public void CreateAttachmentForTask_WithoutUserIdHeader_ReturnsBadRequest()
        {
            var repo = new Mock<IAttachmentRepository>();
            var controller = CreateController(repo, userIdHeader: null);
            var dto = new AttachmentCreationDTO { ProjectId = Guid.NewGuid() };

            var result = controller.CreateAttachmentForTask(Guid.NewGuid(), dto);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        // ---------- ConfirmAttachment ----------

        [Theory]
        [InlineData(ConfirmAttachmentOutcome.NotFound, typeof(NotFoundResult))]
        [InlineData(ConfirmAttachmentOutcome.InvalidState, typeof(ConflictObjectResult))]
        [InlineData(ConfirmAttachmentOutcome.ObjectMissing, typeof(ConflictObjectResult))]
        public async Task ConfirmAttachment_MapsEachFailureOutcomeToExpectedStatusCode(ConfirmAttachmentOutcome outcome, Type expectedResultType)
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.ConfirmAttachmentAsync(It.IsAny<AttachmentConfirmationDTO>()))
                .ReturnsAsync(new ConfirmAttachmentResult(outcome, null));
            var controller = CreateController(repo);

            var result = await controller.ConfirmAttachment(new AttachmentConfirmationDTO { AttachmentId = Guid.NewGuid() });

            Assert.IsType(expectedResultType, result.Result);
        }

        [Fact]
        public async Task ConfirmAttachment_OnSuccess_ReturnsOkWithAttachment()
        {
            var repo = new Mock<IAttachmentRepository>();
            var dto = SampleDto();
            repo.Setup(r => r.ConfirmAttachmentAsync(It.IsAny<AttachmentConfirmationDTO>()))
                .ReturnsAsync(new ConfirmAttachmentResult(ConfirmAttachmentOutcome.Success, dto));
            var controller = CreateController(repo);

            var result = await controller.ConfirmAttachment(new AttachmentConfirmationDTO { AttachmentId = dto.Id });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(dto, ok.Value);
        }

        // ---------- UpdateAttachment ----------

        [Fact]
        public void UpdateAttachment_WhenFound_ReturnsOk()
        {
            var repo = new Mock<IAttachmentRepository>();
            var dto = SampleDto();
            repo.Setup(r => r.UpdateAttachment(dto.Id, It.IsAny<AttachmentUpdateDTO>())).Returns(dto);
            var controller = CreateController(repo);

            var result = controller.UpdateAttachment(dto.Id, new AttachmentUpdateDTO { Description = "x" });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(dto, ok.Value);
        }

        [Fact]
        public void UpdateAttachment_WhenMissing_ReturnsNotFound()
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.UpdateAttachment(It.IsAny<Guid>(), It.IsAny<AttachmentUpdateDTO>())).Returns((AttachmentDTO?)null);
            var controller = CreateController(repo);

            var result = controller.UpdateAttachment(Guid.NewGuid(), new AttachmentUpdateDTO());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // ---------- DeleteAttachment ----------

        [Fact]
        public void DeleteAttachment_WhenFound_ReturnsNoContentAndCallsRepository()
        {
            var repo = new Mock<IAttachmentRepository>();
            var id = Guid.NewGuid();
            repo.Setup(r => r.GetAttachmentById(id)).Returns(SampleDto(id));
            var controller = CreateController(repo);

            var result = controller.DeleteAttachment(id);

            Assert.IsType<NoContentResult>(result);
            repo.Verify(r => r.DeleteAttachment(id), Times.Once);
        }

        [Fact]
        public void DeleteAttachment_WhenMissing_ReturnsNotFoundAndDoesNotCallDelete()
        {
            var repo = new Mock<IAttachmentRepository>();
            repo.Setup(r => r.GetAttachmentById(It.IsAny<Guid>())).Returns((AttachmentDTO?)null);
            var controller = CreateController(repo);

            var result = controller.DeleteAttachment(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
            repo.Verify(r => r.DeleteAttachment(It.IsAny<Guid>()), Times.Never);
        }
    }
}
