using AutoMapper;
using AttachmentService.Context;
using AttachmentService.Data;
using AttachmentService.Exceptions;
using AttachmentService.Models;
using AttachmentService.Models.DTO;
using AttachmentService.Models.DTO.User;
using AttachmentService.Models.DTO.WorkPackage;
using AttachmentService.Models.Enums;
using AttachmentService.Profiles;
using AttachmentService.ServiceCalls.Project;
using AttachmentService.ServiceCalls.User;
using AttachmentService.ServiceCalls.WorkPackage;
using AttachmentService.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AttachmentService.Tests
{
    public class AttachmentRepositoryTests
    {
        private static AttachmentContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AttachmentContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var configuration = new ConfigurationBuilder().Build();

            return new AttachmentContext(options, configuration);
        }

        private static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<AttachmentProfile>(), NullLoggerFactory.Instance);
            return config.CreateMapper();
        }

        private sealed class Fixture
        {
            public AttachmentContext Context { get; }
            public AttachmentRepository Repository { get; }
            public Mock<IFileStorageService> FileStorageService { get; }
            public Mock<ITaskService> TaskService { get; }
            public Mock<IProjectService> ProjectService { get; }
            public Mock<IUserService> UserService { get; }

            public Fixture()
            {
                Context = CreateContext();
                FileStorageService = new Mock<IFileStorageService>();
                TaskService = new Mock<ITaskService>();
                ProjectService = new Mock<IProjectService>();
                UserService = new Mock<IUserService>();

                TaskService.Setup(s => s.GetTaskByIdAsync(It.IsAny<Guid>(), It.IsAny<string?>()))
                    .ReturnsAsync(new TaskLookupResult(TaskLookupStatus.Found, new TaskDTO { Title = "Some Task" }));
                ProjectService.Setup(s => s.CheckProjectExistsAsync(It.IsAny<Guid>(), It.IsAny<string?>()))
                    .ReturnsAsync(new ProjectExistsResult(ProjectExistsStatus.Exists));
                ProjectService.Setup(s => s.CheckMembershipAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                    .ReturnsAsync(new ProjectMembershipResult(ProjectMembershipStatus.Member));
                UserService.Setup(s => s.GetUserInfoAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(new UserInfoDTO { Username = "member.user", Email = "member@example.com", Role = "TeamMember" });

                Repository = new AttachmentRepository(
                    Context,
                    CreateMapper(),
                    FileStorageService.Object,
                    TaskService.Object,
                    ProjectService.Object,
                    UserService.Object);
            }
        }

        // ---------- CreateAttachmentAsync ----------

        [Fact]
        public async Task CreateAttachmentAsync_PopulatesGeneratedFieldsAndPersists()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            fx.FileStorageService
                .Setup(s => s.GenerateUploadUrl(It.IsAny<string>(), "text/plain", null))
                .Returns("https://storage.example/upload-url");

            var dto = new AttachmentCreationDTO
            {
                OriginalFileName = "notes.txt",
                ContentType = "text/plain",
                FileSize = 100,
                ProjectId = projectId
            };

            var result = await fx.Repository.CreateAttachmentAsync(dto, userId, bearerToken: null);

            Assert.NotEqual(Guid.Empty, result.Attachment.Id);
            Assert.Equal(AttachmentStatus.Uploading, result.Attachment.Status);
            Assert.Equal(userId, result.Attachment.UploadedByUserId);
            Assert.Contains(result.Attachment.Id.ToString(), result.Attachment.FileName);
            Assert.Contains("notes.txt", result.Attachment.FileName);
            Assert.Equal("https://storage.example/upload-url", result.UploadUrl);
            Assert.Equal("member.user", result.UploadedByUsername);
            Assert.Equal("TeamMember", result.UploadedByRole);

            var persisted = fx.Context.Attachments.Single(a => a.Id == result.Attachment.Id);
            Assert.Equal(projectId, persisted.ProjectId);
            Assert.Null(persisted.TaskId);
            Assert.StartsWith($"projects/{projectId}/", persisted.StoragePath);
        }

        [Fact]
        public async Task CreateAttachmentAsync_WithTaskId_BuildsNestedStoragePath()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            fx.FileStorageService
                .Setup(s => s.GenerateUploadUrl(It.IsAny<string>(), It.IsAny<string>(), null))
                .Returns("https://storage.example/upload-url");

            var dto = new AttachmentCreationDTO
            {
                OriginalFileName = "notes.txt",
                ContentType = "text/plain",
                FileSize = 100,
                ProjectId = projectId,
                TaskId = taskId
            };

            var result = await fx.Repository.CreateAttachmentAsync(dto, Guid.NewGuid(), bearerToken: null);

            var persisted = fx.Context.Attachments.Single(a => a.Id == result.Attachment.Id);
            Assert.Equal(taskId, persisted.TaskId);
            Assert.Contains($"projects/{projectId}/tasks/{taskId}/", persisted.StoragePath);
        }

        [Fact]
        public async Task CreateAttachmentAsync_WhenProjectConfirmedNotFound_ThrowsAndDoesNotPersist()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            fx.ProjectService.Setup(s => s.CheckProjectExistsAsync(projectId, It.IsAny<string?>()))
                .ReturnsAsync(new ProjectExistsResult(ProjectExistsStatus.NotFound));
            var dto = new AttachmentCreationDTO { OriginalFileName = "a.txt", ContentType = "text/plain", FileSize = 1, ProjectId = projectId };

            await Assert.ThrowsAsync<ProjectNotFoundException>(
                () => fx.Repository.CreateAttachmentAsync(dto, Guid.NewGuid(), bearerToken: null));

            Assert.Empty(fx.Context.Attachments);
        }

        [Fact]
        public async Task CreateAttachmentAsync_WhenTaskConfirmedNotFound_ThrowsAndDoesNotPersist()
        {
            var fx = new Fixture();
            var taskId = Guid.NewGuid();
            fx.TaskService.Setup(s => s.GetTaskByIdAsync(taskId, It.IsAny<string?>()))
                .ReturnsAsync(new TaskLookupResult(TaskLookupStatus.NotFound, null));
            var dto = new AttachmentCreationDTO { OriginalFileName = "a.txt", ContentType = "text/plain", FileSize = 1, ProjectId = Guid.NewGuid(), TaskId = taskId };

            await Assert.ThrowsAsync<TaskNotFoundException>(
                () => fx.Repository.CreateAttachmentAsync(dto, Guid.NewGuid(), bearerToken: null));

            Assert.Empty(fx.Context.Attachments);
        }

        [Fact]
        public async Task CreateAttachmentAsync_WhenUserNotAProjectMember_ThrowsAndDoesNotPersist()
        {
            var fx = new Fixture();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            fx.ProjectService.Setup(s => s.CheckMembershipAsync(projectId, userId, It.IsAny<string?>()))
                .ReturnsAsync(new ProjectMembershipResult(ProjectMembershipStatus.NotMember));
            var dto = new AttachmentCreationDTO { OriginalFileName = "a.txt", ContentType = "text/plain", FileSize = 1, ProjectId = projectId };

            await Assert.ThrowsAsync<UserNotProjectMemberException>(
                () => fx.Repository.CreateAttachmentAsync(dto, userId, bearerToken: null));

            Assert.Empty(fx.Context.Attachments);
        }

        [Fact]
        public async Task CreateAttachmentAsync_WhenUserHasClientRole_ThrowsAndDoesNotPersist()
        {
            var fx = new Fixture();
            var userId = Guid.NewGuid();
            fx.UserService.Setup(s => s.GetUserInfoAsync(userId))
                .ReturnsAsync(new UserInfoDTO { Username = "client.user", Role = "Client" });
            var dto = new AttachmentCreationDTO { OriginalFileName = "a.txt", ContentType = "text/plain", FileSize = 1, ProjectId = Guid.NewGuid() };

            await Assert.ThrowsAsync<RoleCannotUploadAttachmentsException>(
                () => fx.Repository.CreateAttachmentAsync(dto, userId, bearerToken: null));

            Assert.Empty(fx.Context.Attachments);
        }

        [Theory]
        [InlineData("TeamMember")]
        [InlineData("ProjectManager")]
        public async Task CreateAttachmentAsync_WithUploaderRole_Succeeds(string role)
        {
            var fx = new Fixture();
            var userId = Guid.NewGuid();
            fx.UserService.Setup(s => s.GetUserInfoAsync(userId))
                .ReturnsAsync(new UserInfoDTO { Username = "u", Role = role });
            var dto = new AttachmentCreationDTO { OriginalFileName = "a.txt", ContentType = "text/plain", FileSize = 1, ProjectId = Guid.NewGuid() };

            var result = await fx.Repository.CreateAttachmentAsync(dto, userId, bearerToken: null);

            Assert.NotEqual(Guid.Empty, result.Attachment.Id);
        }

        [Fact]
        public async Task CreateAttachmentAsync_WhenUserIsAdmin_BypassesRoleAndMembershipChecks()
        {
            var fx = new Fixture();
            var userId = Guid.NewGuid();
            fx.UserService.Setup(s => s.GetUserInfoAsync(userId))
                .ReturnsAsync(new UserInfoDTO { Username = "admin.user", Role = "Admin" });
            fx.ProjectService.Setup(s => s.CheckMembershipAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ReturnsAsync(new ProjectMembershipResult(ProjectMembershipStatus.NotMember));
            var dto = new AttachmentCreationDTO { OriginalFileName = "a.txt", ContentType = "text/plain", FileSize = 1, ProjectId = Guid.NewGuid() };

            var result = await fx.Repository.CreateAttachmentAsync(dto, userId, bearerToken: null);

            Assert.NotEqual(Guid.Empty, result.Attachment.Id);
            fx.ProjectService.Verify(s => s.CheckMembershipAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>()), Times.Never);
        }

        // ---------- GetAttachmentsAsync (filtering + view authorization) ----------

        [Fact]
        public async Task GetAttachmentsAsync_FilteredByProjectId_ReturnsProjectAndItsTaskAttachments()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            var otherProjectId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            fx.Context.Attachments.AddRange(
                new Attachment { Id = Guid.NewGuid(), ProjectId = projectId, TaskId = null, Status = AttachmentStatus.Ready, FileName = "a" },
                new Attachment { Id = Guid.NewGuid(), ProjectId = projectId, TaskId = taskId, Status = AttachmentStatus.Ready, FileName = "b" },
                new Attachment { Id = Guid.NewGuid(), ProjectId = otherProjectId, TaskId = null, Status = AttachmentStatus.Ready, FileName = "c" }
            );
            fx.Context.SaveChanges();

            var result = (await fx.Repository.GetAttachmentsAsync(projectId, null, Guid.NewGuid(), null)).ToList();

            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(result, a => a.ProjectId == otherProjectId);
        }

        [Fact]
        public async Task GetAttachmentsAsync_FilteredByTaskId_ReturnsOnlyThatTask()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var otherTaskId = Guid.NewGuid();

            fx.Context.Attachments.AddRange(
                new Attachment { Id = Guid.NewGuid(), ProjectId = projectId, TaskId = null, Status = AttachmentStatus.Ready, FileName = "a" },
                new Attachment { Id = Guid.NewGuid(), ProjectId = projectId, TaskId = taskId, Status = AttachmentStatus.Ready, FileName = "b" },
                new Attachment { Id = Guid.NewGuid(), ProjectId = projectId, TaskId = otherTaskId, Status = AttachmentStatus.Ready, FileName = "c" }
            );
            fx.Context.SaveChanges();

            var result = (await fx.Repository.GetAttachmentsAsync(null, taskId, Guid.NewGuid(), null)).ToList();

            Assert.Single(result);
            Assert.Equal(taskId, result[0].TaskId);
        }

        [Fact]
        public async Task GetAttachmentsAsync_ExcludesSoftDeleted()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();

            fx.Context.Attachments.AddRange(
                new Attachment { Id = Guid.NewGuid(), ProjectId = projectId, Status = AttachmentStatus.Ready, FileName = "a" },
                new Attachment { Id = Guid.NewGuid(), ProjectId = projectId, Status = AttachmentStatus.Deleted, FileName = "b" }
            );
            fx.Context.SaveChanges();

            var result = (await fx.Repository.GetAttachmentsAsync(projectId, null, Guid.NewGuid(), null)).ToList();

            Assert.Single(result);
            Assert.Equal("a", result[0].FileName);
        }

        [Fact]
        public async Task GetAttachmentsAsync_WhenUserNotAProjectMember_ThrowsForbidden()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = Guid.NewGuid(), ProjectId = projectId, Status = AttachmentStatus.Ready, FileName = "a" });
            fx.Context.SaveChanges();
            fx.ProjectService.Setup(s => s.CheckMembershipAsync(projectId, userId, It.IsAny<string?>()))
                .ReturnsAsync(new ProjectMembershipResult(ProjectMembershipStatus.NotMember));

            await Assert.ThrowsAsync<UserNotProjectMemberException>(
                () => fx.Repository.GetAttachmentsAsync(projectId, null, userId, null));
        }

        [Fact]
        public async Task GetAttachmentsAsync_WithClientRole_StillCanView()
        {
            // View/download is open to any active member regardless of role - only Upload is
            // role-restricted.
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = Guid.NewGuid(), ProjectId = projectId, Status = AttachmentStatus.Ready, FileName = "a" });
            fx.Context.SaveChanges();
            fx.UserService.Setup(s => s.GetUserInfoAsync(userId))
                .ReturnsAsync(new UserInfoDTO { Username = "client.user", Role = "Client" });

            var result = await fx.Repository.GetAttachmentsAsync(projectId, null, userId, null);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetAttachmentsAsync_WithNoFilter_RequiresConfirmedAdmin()
        {
            var fx = new Fixture();
            var userId = Guid.NewGuid();
            fx.UserService.Setup(s => s.GetUserInfoAsync(userId)).ReturnsAsync((UserInfoDTO?)null);

            await Assert.ThrowsAsync<ProjectContextRequiredException>(
                () => fx.Repository.GetAttachmentsAsync(null, null, userId, null));
        }

        [Fact]
        public async Task GetAttachmentsAsync_WithNoFilter_AsAdmin_Succeeds()
        {
            var fx = new Fixture();
            var userId = Guid.NewGuid();
            fx.UserService.Setup(s => s.GetUserInfoAsync(userId))
                .ReturnsAsync(new UserInfoDTO { Username = "admin.user", Role = "Admin" });
            fx.Context.Attachments.Add(new Attachment { Id = Guid.NewGuid(), ProjectId = Guid.NewGuid(), Status = AttachmentStatus.Ready, FileName = "a" });
            fx.Context.SaveChanges();

            var result = await fx.Repository.GetAttachmentsAsync(null, null, userId, null);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetAttachmentsAsync_FilteredByTaskIdWithNoResults_ReturnsEmptyWithoutCheckingMembership()
        {
            var fx = new Fixture();

            var result = await fx.Repository.GetAttachmentsAsync(null, Guid.NewGuid(), Guid.NewGuid(), null);

            Assert.Empty(result);
            fx.ProjectService.Verify(s => s.CheckMembershipAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>()), Times.Never);
        }

        // ---------- GetAttachmentByIdAsync ----------

        [Fact]
        public async Task GetAttachmentByIdAsync_ForExistingAttachment_ReturnsIt()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, ProjectId = Guid.NewGuid(), Status = AttachmentStatus.Ready, FileName = "a" });
            fx.Context.SaveChanges();

            var result = await fx.Repository.GetAttachmentByIdAsync(id, Guid.NewGuid(), null);

            Assert.NotNull(result);
            Assert.Equal(id, result!.Id);
        }

        [Fact]
        public async Task GetAttachmentByIdAsync_ForNonexistentId_ReturnsNull()
        {
            var fx = new Fixture();

            var result = await fx.Repository.GetAttachmentByIdAsync(Guid.NewGuid(), Guid.NewGuid(), null);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAttachmentByIdAsync_ForSoftDeletedAttachment_ReturnsNull()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Deleted, FileName = "a" });
            fx.Context.SaveChanges();

            var result = await fx.Repository.GetAttachmentByIdAsync(id, Guid.NewGuid(), null);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAttachmentByIdAsync_WhenUserNotAProjectMember_ThrowsForbidden()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, ProjectId = projectId, Status = AttachmentStatus.Ready, FileName = "a" });
            fx.Context.SaveChanges();
            fx.ProjectService.Setup(s => s.CheckMembershipAsync(projectId, userId, It.IsAny<string?>()))
                .ReturnsAsync(new ProjectMembershipResult(ProjectMembershipStatus.NotMember));

            await Assert.ThrowsAsync<UserNotProjectMemberException>(
                () => fx.Repository.GetAttachmentByIdAsync(id, userId, null));
        }

        // ---------- GetDownloadUrlAsync ----------

        [Fact]
        public async Task GetDownloadUrlAsync_ForReadyAttachment_ReturnsPresignedUrl()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, ProjectId = Guid.NewGuid(), Status = AttachmentStatus.Ready, StoragePath = "path/to/file", FileName = "a" });
            fx.Context.SaveChanges();
            fx.FileStorageService.Setup(s => s.GenerateDownloadUrl("path/to/file", null)).Returns("https://storage.example/download-url");

            var result = await fx.Repository.GetDownloadUrlAsync(id, Guid.NewGuid(), null);

            Assert.Equal("https://storage.example/download-url", result);
        }

        [Fact]
        public async Task GetDownloadUrlAsync_ForAttachmentStillUploading_ReturnsNull()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Uploading, FileName = "a" });
            fx.Context.SaveChanges();

            var result = await fx.Repository.GetDownloadUrlAsync(id, Guid.NewGuid(), null);

            Assert.Null(result);
            fx.FileStorageService.Verify(s => s.GenerateDownloadUrl(It.IsAny<string>(), It.IsAny<TimeSpan?>()), Times.Never);
        }

        [Fact]
        public async Task GetDownloadUrlAsync_WithClientRole_StillSucceeds()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, ProjectId = Guid.NewGuid(), Status = AttachmentStatus.Ready, StoragePath = "path/x", FileName = "a" });
            fx.Context.SaveChanges();
            fx.UserService.Setup(s => s.GetUserInfoAsync(userId))
                .ReturnsAsync(new UserInfoDTO { Username = "client.user", Role = "Client" });
            fx.FileStorageService.Setup(s => s.GenerateDownloadUrl("path/x", null)).Returns("https://storage.example/download-url");

            var result = await fx.Repository.GetDownloadUrlAsync(id, userId, null);

            Assert.Equal("https://storage.example/download-url", result);
        }

        [Fact]
        public async Task GetDownloadUrlAsync_WhenUserNotAProjectMember_ThrowsForbidden()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, ProjectId = projectId, Status = AttachmentStatus.Ready, StoragePath = "path/x", FileName = "a" });
            fx.Context.SaveChanges();
            fx.ProjectService.Setup(s => s.CheckMembershipAsync(projectId, userId, It.IsAny<string?>()))
                .ReturnsAsync(new ProjectMembershipResult(ProjectMembershipStatus.NotMember));

            await Assert.ThrowsAsync<UserNotProjectMemberException>(
                () => fx.Repository.GetDownloadUrlAsync(id, userId, null));
        }

        // ---------- ConfirmAttachmentAsync ----------

        [Fact]
        public async Task ConfirmAttachmentAsync_ByOwner_WhenObjectExists_MarksReadyAndSetsChecksum()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var uploaderId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Uploading, StoragePath = "path/x", FileName = "a", UploadedByUserId = uploaderId });
            fx.Context.SaveChanges();
            fx.FileStorageService.Setup(s => s.ObjectExistsAsync("path/x")).ReturnsAsync(true);
            var checksum = new string('a', 64);

            var result = await fx.Repository.ConfirmAttachmentAsync(new AttachmentConfirmationDTO { AttachmentId = id, Checksum = checksum }, uploaderId, null);

            Assert.Equal(ConfirmAttachmentOutcome.Success, result.Outcome);
            Assert.Equal(AttachmentStatus.Ready, result.Attachment!.Status);
            Assert.Equal(checksum, result.Attachment.Checksum);

            var persisted = fx.Context.Attachments.Single(a => a.Id == id);
            Assert.Equal(AttachmentStatus.Ready, persisted.Status);
        }

        [Fact]
        public async Task ConfirmAttachmentAsync_ByNonOwnerNonAdmin_ReturnsForbidden()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var uploaderId = Guid.NewGuid();
            var actingUserId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Uploading, StoragePath = "path/x", FileName = "a", UploadedByUserId = uploaderId });
            fx.Context.SaveChanges();

            var result = await fx.Repository.ConfirmAttachmentAsync(new AttachmentConfirmationDTO { AttachmentId = id }, actingUserId, null);

            Assert.Equal(ConfirmAttachmentOutcome.Forbidden, result.Outcome);
            var persisted = fx.Context.Attachments.Single(a => a.Id == id);
            Assert.Equal(AttachmentStatus.Uploading, persisted.Status);
        }

        [Fact]
        public async Task ConfirmAttachmentAsync_ByAdmin_Succeeds()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var uploaderId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Uploading, StoragePath = "path/x", FileName = "a", UploadedByUserId = uploaderId });
            fx.Context.SaveChanges();
            fx.UserService.Setup(s => s.GetUserInfoAsync(adminId))
                .ReturnsAsync(new UserInfoDTO { Username = "admin.user", Role = "Admin" });
            fx.FileStorageService.Setup(s => s.ObjectExistsAsync("path/x")).ReturnsAsync(true);

            var result = await fx.Repository.ConfirmAttachmentAsync(new AttachmentConfirmationDTO { AttachmentId = id }, adminId, null);

            Assert.Equal(ConfirmAttachmentOutcome.Success, result.Outcome);
        }

        [Fact]
        public async Task ConfirmAttachmentAsync_ForNonexistentId_ReturnsNotFound()
        {
            var fx = new Fixture();

            var result = await fx.Repository.ConfirmAttachmentAsync(new AttachmentConfirmationDTO { AttachmentId = Guid.NewGuid() }, Guid.NewGuid(), null);

            Assert.Equal(ConfirmAttachmentOutcome.NotFound, result.Outcome);
            Assert.Null(result.Attachment);
        }

        [Fact]
        public async Task ConfirmAttachmentAsync_ForSoftDeletedAttachment_ReturnsNotFound()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Deleted, FileName = "a" });
            fx.Context.SaveChanges();

            var result = await fx.Repository.ConfirmAttachmentAsync(new AttachmentConfirmationDTO { AttachmentId = id }, Guid.NewGuid(), null);

            Assert.Equal(ConfirmAttachmentOutcome.NotFound, result.Outcome);
        }

        [Fact]
        public async Task ConfirmAttachmentAsync_ForAlreadyReadyAttachment_ReturnsInvalidState()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var uploaderId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Ready, FileName = "a", UploadedByUserId = uploaderId });
            fx.Context.SaveChanges();

            var result = await fx.Repository.ConfirmAttachmentAsync(new AttachmentConfirmationDTO { AttachmentId = id }, uploaderId, null);

            Assert.Equal(ConfirmAttachmentOutcome.InvalidState, result.Outcome);
            fx.FileStorageService.Verify(s => s.ObjectExistsAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmAttachmentAsync_WhenObjectMissingFromStorage_ReturnsObjectMissingAndLeavesStatusUnchanged()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var uploaderId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Uploading, StoragePath = "path/x", FileName = "a", UploadedByUserId = uploaderId });
            fx.Context.SaveChanges();
            fx.FileStorageService.Setup(s => s.ObjectExistsAsync("path/x")).ReturnsAsync(false);

            var result = await fx.Repository.ConfirmAttachmentAsync(new AttachmentConfirmationDTO { AttachmentId = id }, uploaderId, null);

            Assert.Equal(ConfirmAttachmentOutcome.ObjectMissing, result.Outcome);
            Assert.Null(result.Attachment);

            var persisted = fx.Context.Attachments.Single(a => a.Id == id);
            Assert.Equal(AttachmentStatus.Uploading, persisted.Status);
        }

        // ---------- UpdateAttachmentAsync ----------

        [Fact]
        public async Task UpdateAttachmentAsync_ByOwner_WithDescriptionOnly_LeavesFileNameUntouched()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var uploaderId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, FileName = "original.txt", Description = "old", Status = AttachmentStatus.Ready, UploadedByUserId = uploaderId });
            fx.Context.SaveChanges();

            var result = await fx.Repository.UpdateAttachmentAsync(id, new AttachmentUpdateDTO { Description = "new description" }, uploaderId, null);

            Assert.NotNull(result);
            Assert.Equal("original.txt", result!.FileName);
            Assert.Equal("new description", result.Description);
        }

        [Fact]
        public async Task UpdateAttachmentAsync_ForNonexistentId_ReturnsNull()
        {
            var fx = new Fixture();

            var result = await fx.Repository.UpdateAttachmentAsync(Guid.NewGuid(), new AttachmentUpdateDTO { Description = "x" }, Guid.NewGuid(), null);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAttachmentAsync_ByNonOwnerNonAdmin_ThrowsAndDoesNotPersist()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var uploaderId = Guid.NewGuid();
            var actingUserId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, FileName = "original.txt", Status = AttachmentStatus.Ready, UploadedByUserId = uploaderId });
            fx.Context.SaveChanges();

            await Assert.ThrowsAsync<NotAttachmentOwnerException>(
                () => fx.Repository.UpdateAttachmentAsync(id, new AttachmentUpdateDTO { Description = "hacked" }, actingUserId, null));

            var persisted = fx.Context.Attachments.Single(a => a.Id == id);
            Assert.Null(persisted.Description);
        }

        [Fact]
        public async Task UpdateAttachmentAsync_ByAdmin_Succeeds()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var uploaderId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, FileName = "original.txt", Status = AttachmentStatus.Ready, UploadedByUserId = uploaderId });
            fx.Context.SaveChanges();
            fx.UserService.Setup(s => s.GetUserInfoAsync(adminId))
                .ReturnsAsync(new UserInfoDTO { Username = "admin.user", Role = "Admin" });

            var result = await fx.Repository.UpdateAttachmentAsync(id, new AttachmentUpdateDTO { Description = "fixed by admin" }, adminId, null);

            Assert.Equal("fixed by admin", result!.Description);
        }

        // ---------- DeleteAttachmentAsync ----------

        [Fact]
        public async Task DeleteAttachmentAsync_ByOwner_SoftDeletesAndSetsDeletedAt()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var uploaderId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Ready, FileName = "a", UploadedByUserId = uploaderId });
            fx.Context.SaveChanges();

            var deleted = await fx.Repository.DeleteAttachmentAsync(id, uploaderId, null);

            Assert.True(deleted);
            var persisted = fx.Context.Attachments.Single(a => a.Id == id);
            Assert.Equal(AttachmentStatus.Deleted, persisted.Status);
            Assert.NotNull(persisted.DeletedAt);
        }

        [Fact]
        public async Task DeleteAttachmentAsync_ByNonOwnerNonAdmin_ThrowsAndDoesNotDelete()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var uploaderId = Guid.NewGuid();
            var actingUserId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Ready, FileName = "a", UploadedByUserId = uploaderId });
            fx.Context.SaveChanges();

            await Assert.ThrowsAsync<NotAttachmentOwnerException>(
                () => fx.Repository.DeleteAttachmentAsync(id, actingUserId, null));

            var persisted = fx.Context.Attachments.Single(a => a.Id == id);
            Assert.Equal(AttachmentStatus.Ready, persisted.Status);
        }

        [Fact]
        public async Task DeleteAttachmentAsync_ByAdmin_Succeeds()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var uploaderId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Ready, FileName = "a", UploadedByUserId = uploaderId });
            fx.Context.SaveChanges();
            fx.UserService.Setup(s => s.GetUserInfoAsync(adminId))
                .ReturnsAsync(new UserInfoDTO { Username = "admin.user", Role = "Admin" });

            var deleted = await fx.Repository.DeleteAttachmentAsync(id, adminId, null);

            Assert.True(deleted);
        }

        [Fact]
        public async Task DeleteAttachmentAsync_ForNonexistentId_ReturnsFalseWithoutThrowing()
        {
            var fx = new Fixture();

            var deleted = await fx.Repository.DeleteAttachmentAsync(Guid.NewGuid(), Guid.NewGuid(), null);

            Assert.False(deleted);
        }

        [Fact]
        public async Task DeleteAttachmentAsync_CalledTwice_IsIdempotentAndSkipsOwnershipCheckOnSecondCall()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var uploaderId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Ready, FileName = "a", UploadedByUserId = uploaderId });
            fx.Context.SaveChanges();

            await fx.Repository.DeleteAttachmentAsync(id, uploaderId, null);
            var firstDeletedAt = fx.Context.Attachments.Single(a => a.Id == id).DeletedAt;

            // A different, non-owning caller can "delete" an already-deleted row without error -
            // there's nothing left to protect.
            var secondDeleted = await fx.Repository.DeleteAttachmentAsync(id, Guid.NewGuid(), null);
            var secondDeletedAt = fx.Context.Attachments.Single(a => a.Id == id).DeletedAt;

            Assert.True(secondDeleted);
            Assert.Equal(firstDeletedAt, secondDeletedAt);
        }

        // ---------- GetAttachmentDetailsAsync ----------

        [Fact]
        public async Task GetAttachmentDetailsAsync_WithTaskAndUploader_EnrichesBoth()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment
            {
                Id = id,
                ProjectId = Guid.NewGuid(),
                Status = AttachmentStatus.Ready,
                FileName = "a",
                TaskId = taskId,
                UploadedByUserId = userId
            });
            fx.Context.SaveChanges();
            fx.TaskService.Setup(s => s.GetTaskByIdAsync(taskId, It.IsAny<string?>()))
                .ReturnsAsync(new TaskLookupResult(TaskLookupStatus.Found, new TaskDTO { Title = "Fix the bug" }));
            fx.UserService.Setup(s => s.GetUserInfoAsync(userId))
                .ReturnsAsync(new UserInfoDTO { Username = "mila", Role = "TeamMember" });

            var result = await fx.Repository.GetAttachmentDetailsAsync(id, Guid.NewGuid(), null);

            Assert.NotNull(result);
            Assert.Equal("Fix the bug", result!.TaskTitle);
            Assert.Equal("mila", result.UploadedByUsername);
            Assert.Equal("TeamMember", result.UploadedByRole);
        }

        [Fact]
        public async Task GetAttachmentDetailsAsync_WithoutTask_DoesNotCallTaskService()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, ProjectId = Guid.NewGuid(), Status = AttachmentStatus.Ready, FileName = "a", TaskId = null });
            fx.Context.SaveChanges();

            var result = await fx.Repository.GetAttachmentDetailsAsync(id, Guid.NewGuid(), null);

            Assert.NotNull(result);
            Assert.Null(result!.TaskTitle);
            fx.TaskService.Verify(s => s.GetTaskByIdAsync(It.IsAny<Guid>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task GetAttachmentDetailsAsync_ForNonexistentId_ReturnsNull()
        {
            var fx = new Fixture();

            var result = await fx.Repository.GetAttachmentDetailsAsync(Guid.NewGuid(), Guid.NewGuid(), null);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAttachmentDetailsAsync_WhenUserNotAProjectMember_ThrowsForbidden()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, ProjectId = projectId, Status = AttachmentStatus.Ready, FileName = "a" });
            fx.Context.SaveChanges();
            fx.ProjectService.Setup(s => s.CheckMembershipAsync(projectId, userId, It.IsAny<string?>()))
                .ReturnsAsync(new ProjectMembershipResult(ProjectMembershipStatus.NotMember));

            await Assert.ThrowsAsync<UserNotProjectMemberException>(
                () => fx.Repository.GetAttachmentDetailsAsync(id, userId, null));
        }
    }
}
