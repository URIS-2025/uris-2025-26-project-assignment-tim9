using AutoMapper;
using AttachmentService.Context;
using AttachmentService.Data;
using AttachmentService.Models;
using AttachmentService.Models.DTO;
using AttachmentService.Models.DTO.Project;
using AttachmentService.Models.DTO.WorkPackage;
using AttachmentService.Models.Enums;
using AttachmentService.Profiles;
using AttachmentService.ServiceCalls.Project;
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
            // Fresh, isolated in-memory database per test (unique name), so tests never see
            // each other's data or the OnModelCreating seed rows' side effects bleed across
            // tests. IConfiguration is never actually touched because options are already
            // configured (OnConfiguring's guard skips it), so an empty one is fine here.
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
            public Mock<IWorkPackageService> WorkPackageService { get; }
            public Mock<IProjectService> ProjectService { get; }

            public Fixture()
            {
                Context = CreateContext();
                FileStorageService = new Mock<IFileStorageService>();
                WorkPackageService = new Mock<IWorkPackageService>();
                ProjectService = new Mock<IProjectService>();

                Repository = new AttachmentRepository(
                    Context,
                    CreateMapper(),
                    FileStorageService.Object,
                    WorkPackageService.Object,
                    ProjectService.Object);
            }
        }

        // ---------- CreateAttachment ----------

        [Fact]
        public void CreateAttachment_PopulatesGeneratedFieldsAndPersists()
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

            var result = fx.Repository.CreateAttachment(dto, userId);

            Assert.NotEqual(Guid.Empty, result.Attachment.Id);
            Assert.Equal(AttachmentStatus.Uploading, result.Attachment.Status);
            Assert.Equal(userId, result.Attachment.UploadedByUserId);
            Assert.Contains(result.Attachment.Id.ToString(), result.Attachment.FileName);
            Assert.Contains("notes.txt", result.Attachment.FileName);
            Assert.Equal("https://storage.example/upload-url", result.UploadUrl);

            var persisted = fx.Context.Attachments.Single(a => a.Id == result.Attachment.Id);
            Assert.Equal(projectId, persisted.ProjectId);
            Assert.Null(persisted.WorkPackageId);
            Assert.StartsWith($"projects/{projectId}/", persisted.StoragePath);
        }

        [Fact]
        public void CreateAttachment_WithWorkPackageId_BuildsNestedStoragePath()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            var workPackageId = Guid.NewGuid();
            fx.FileStorageService
                .Setup(s => s.GenerateUploadUrl(It.IsAny<string>(), It.IsAny<string>(), null))
                .Returns("https://storage.example/upload-url");

            var dto = new AttachmentCreationDTO
            {
                OriginalFileName = "notes.txt",
                ContentType = "text/plain",
                FileSize = 100,
                ProjectId = projectId,
                WorkPackageId = workPackageId
            };

            var result = fx.Repository.CreateAttachment(dto, Guid.NewGuid());

            var persisted = fx.Context.Attachments.Single(a => a.Id == result.Attachment.Id);
            Assert.Equal(workPackageId, persisted.WorkPackageId);
            Assert.Contains($"projects/{projectId}/workpackages/{workPackageId}/", persisted.StoragePath);
        }

        // ---------- GetAttachments (filtering) ----------

        [Fact]
        public void GetAttachments_FilteredByProjectId_ReturnsProjectAndItsWorkPackageAttachments()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            var otherProjectId = Guid.NewGuid();
            var workPackageId = Guid.NewGuid();

            fx.Context.Attachments.AddRange(
                new Attachment { Id = Guid.NewGuid(), ProjectId = projectId, WorkPackageId = null, Status = AttachmentStatus.Ready, FileName = "a" },
                new Attachment { Id = Guid.NewGuid(), ProjectId = projectId, WorkPackageId = workPackageId, Status = AttachmentStatus.Ready, FileName = "b" },
                new Attachment { Id = Guid.NewGuid(), ProjectId = otherProjectId, WorkPackageId = null, Status = AttachmentStatus.Ready, FileName = "c" }
            );
            fx.Context.SaveChanges();

            var result = fx.Repository.GetAttachments(projectId: projectId).ToList();

            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(result, a => a.ProjectId == otherProjectId);
        }

        [Fact]
        public void GetAttachments_FilteredByWorkPackageId_ReturnsOnlyThatWorkPackage()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            var workPackageId = Guid.NewGuid();
            var otherWorkPackageId = Guid.NewGuid();

            fx.Context.Attachments.AddRange(
                new Attachment { Id = Guid.NewGuid(), ProjectId = projectId, WorkPackageId = null, Status = AttachmentStatus.Ready, FileName = "a" },
                new Attachment { Id = Guid.NewGuid(), ProjectId = projectId, WorkPackageId = workPackageId, Status = AttachmentStatus.Ready, FileName = "b" },
                new Attachment { Id = Guid.NewGuid(), ProjectId = projectId, WorkPackageId = otherWorkPackageId, Status = AttachmentStatus.Ready, FileName = "c" }
            );
            fx.Context.SaveChanges();

            var result = fx.Repository.GetAttachments(workPackageId: workPackageId).ToList();

            Assert.Single(result);
            Assert.Equal(workPackageId, result[0].WorkPackageId);
        }

        [Fact]
        public void GetAttachments_ExcludesSoftDeleted()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();

            fx.Context.Attachments.AddRange(
                new Attachment { Id = Guid.NewGuid(), ProjectId = projectId, Status = AttachmentStatus.Ready, FileName = "a" },
                new Attachment { Id = Guid.NewGuid(), ProjectId = projectId, Status = AttachmentStatus.Deleted, FileName = "b" }
            );
            fx.Context.SaveChanges();

            var result = fx.Repository.GetAttachments(projectId: projectId).ToList();

            Assert.Single(result);
            Assert.Equal("a", result[0].FileName);
        }

        // ---------- GetAttachmentById ----------

        [Fact]
        public void GetAttachmentById_ForExistingAttachment_ReturnsIt()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Ready, FileName = "a" });
            fx.Context.SaveChanges();

            var result = fx.Repository.GetAttachmentById(id);

            Assert.NotNull(result);
            Assert.Equal(id, result!.Id);
        }

        [Fact]
        public void GetAttachmentById_ForNonexistentId_ReturnsNull()
        {
            var fx = new Fixture();

            var result = fx.Repository.GetAttachmentById(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public void GetAttachmentById_ForSoftDeletedAttachment_ReturnsNull()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Deleted, FileName = "a" });
            fx.Context.SaveChanges();

            var result = fx.Repository.GetAttachmentById(id);

            Assert.Null(result);
        }

        // ---------- GetDownloadUrl ----------

        [Fact]
        public void GetDownloadUrl_ForReadyAttachment_ReturnsPresignedUrl()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Ready, StoragePath = "path/to/file", FileName = "a" });
            fx.Context.SaveChanges();
            fx.FileStorageService.Setup(s => s.GenerateDownloadUrl("path/to/file", null)).Returns("https://storage.example/download-url");

            var result = fx.Repository.GetDownloadUrl(id);

            Assert.Equal("https://storage.example/download-url", result);
        }

        [Fact]
        public void GetDownloadUrl_ForAttachmentStillUploading_ReturnsNull()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Uploading, FileName = "a" });
            fx.Context.SaveChanges();

            var result = fx.Repository.GetDownloadUrl(id);

            Assert.Null(result);
            fx.FileStorageService.Verify(s => s.GenerateDownloadUrl(It.IsAny<string>(), It.IsAny<TimeSpan?>()), Times.Never);
        }

        // ---------- ConfirmAttachmentAsync ----------

        [Fact]
        public async Task ConfirmAttachmentAsync_WhenObjectExists_MarksReadyAndSetsChecksum()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Uploading, StoragePath = "path/x", FileName = "a" });
            fx.Context.SaveChanges();
            fx.FileStorageService.Setup(s => s.ObjectExistsAsync("path/x")).ReturnsAsync(true);
            var checksum = new string('a', 64);

            var result = await fx.Repository.ConfirmAttachmentAsync(new AttachmentConfirmationDTO { AttachmentId = id, Checksum = checksum });

            Assert.Equal(ConfirmAttachmentOutcome.Success, result.Outcome);
            Assert.Equal(AttachmentStatus.Ready, result.Attachment!.Status);
            Assert.Equal(checksum, result.Attachment.Checksum);

            var persisted = fx.Context.Attachments.Single(a => a.Id == id);
            Assert.Equal(AttachmentStatus.Ready, persisted.Status);
        }

        [Fact]
        public async Task ConfirmAttachmentAsync_ForNonexistentId_ReturnsNotFound()
        {
            var fx = new Fixture();

            var result = await fx.Repository.ConfirmAttachmentAsync(new AttachmentConfirmationDTO { AttachmentId = Guid.NewGuid() });

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

            var result = await fx.Repository.ConfirmAttachmentAsync(new AttachmentConfirmationDTO { AttachmentId = id });

            Assert.Equal(ConfirmAttachmentOutcome.NotFound, result.Outcome);
        }

        [Fact]
        public async Task ConfirmAttachmentAsync_ForAlreadyReadyAttachment_ReturnsInvalidState()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Ready, FileName = "a" });
            fx.Context.SaveChanges();

            var result = await fx.Repository.ConfirmAttachmentAsync(new AttachmentConfirmationDTO { AttachmentId = id });

            Assert.Equal(ConfirmAttachmentOutcome.InvalidState, result.Outcome);
            fx.FileStorageService.Verify(s => s.ObjectExistsAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmAttachmentAsync_WhenObjectMissingFromStorage_ReturnsObjectMissingAndLeavesStatusUnchanged()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Uploading, StoragePath = "path/x", FileName = "a" });
            fx.Context.SaveChanges();
            fx.FileStorageService.Setup(s => s.ObjectExistsAsync("path/x")).ReturnsAsync(false);

            var result = await fx.Repository.ConfirmAttachmentAsync(new AttachmentConfirmationDTO { AttachmentId = id });

            Assert.Equal(ConfirmAttachmentOutcome.ObjectMissing, result.Outcome);
            Assert.Null(result.Attachment);

            var persisted = fx.Context.Attachments.Single(a => a.Id == id);
            Assert.Equal(AttachmentStatus.Uploading, persisted.Status);
        }

        // ---------- UpdateAttachment ----------

        [Fact]
        public void UpdateAttachment_WithDescriptionOnly_LeavesFileNameUntouched()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, FileName = "original.txt", Description = "old", Status = AttachmentStatus.Ready });
            fx.Context.SaveChanges();

            var result = fx.Repository.UpdateAttachment(id, new AttachmentUpdateDTO { Description = "new description" });

            Assert.NotNull(result);
            Assert.Equal("original.txt", result!.FileName);
            Assert.Equal("new description", result.Description);
        }

        [Fact]
        public void UpdateAttachment_ForNonexistentId_ReturnsNull()
        {
            var fx = new Fixture();

            var result = fx.Repository.UpdateAttachment(Guid.NewGuid(), new AttachmentUpdateDTO { Description = "x" });

            Assert.Null(result);
        }

        // ---------- DeleteAttachment ----------

        [Fact]
        public void DeleteAttachment_SoftDeletesAndSetsDeletedAt()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Ready, FileName = "a" });
            fx.Context.SaveChanges();

            fx.Repository.DeleteAttachment(id);

            var persisted = fx.Context.Attachments.Single(a => a.Id == id);
            Assert.Equal(AttachmentStatus.Deleted, persisted.Status);
            Assert.NotNull(persisted.DeletedAt);
        }

        [Fact]
        public void DeleteAttachment_CalledTwice_IsIdempotent()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Ready, FileName = "a" });
            fx.Context.SaveChanges();

            fx.Repository.DeleteAttachment(id);
            var firstDeletedAt = fx.Context.Attachments.Single(a => a.Id == id).DeletedAt;

            fx.Repository.DeleteAttachment(id);
            var secondDeletedAt = fx.Context.Attachments.Single(a => a.Id == id).DeletedAt;

            Assert.Equal(firstDeletedAt, secondDeletedAt);
        }

        // ---------- GetAttachmentDetailsAsync ----------

        [Fact]
        public async Task GetAttachmentDetailsAsync_WithWorkPackageAndUploader_EnrichesBoth()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var workPackageId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment
            {
                Id = id,
                Status = AttachmentStatus.Ready,
                FileName = "a",
                WorkPackageId = workPackageId,
                UploadedByUserId = userId
            });
            fx.Context.SaveChanges();
            fx.WorkPackageService.Setup(s => s.GetWorkPackageByIdAsync(workPackageId))
                .ReturnsAsync(new WorkPackageDTO { Title = "Fix the bug" });
            fx.ProjectService.Setup(s => s.GetUserInfoAsync(userId))
                .ReturnsAsync(new UserInfoDTO { Username = "mila", Role = "Developer" });

            var result = await fx.Repository.GetAttachmentDetailsAsync(id);

            Assert.NotNull(result);
            Assert.Equal("Fix the bug", result!.WorkPackageTitle);
            Assert.Equal("mila", result.UploadedByUsername);
            Assert.Equal("Developer", result.UploadedByRole);
        }

        [Fact]
        public async Task GetAttachmentDetailsAsync_WithoutWorkPackage_DoesNotCallWorkPackageService()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Ready, FileName = "a", WorkPackageId = null });
            fx.Context.SaveChanges();
            fx.ProjectService.Setup(s => s.GetUserInfoAsync(It.IsAny<Guid>())).ReturnsAsync((UserInfoDTO?)null);

            var result = await fx.Repository.GetAttachmentDetailsAsync(id);

            Assert.NotNull(result);
            Assert.Null(result!.WorkPackageTitle);
            fx.WorkPackageService.Verify(s => s.GetWorkPackageByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetAttachmentDetailsAsync_WhenDependenciesReturnNull_DegradesGracefully()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var workPackageId = Guid.NewGuid();
            fx.Context.Attachments.Add(new Attachment { Id = id, Status = AttachmentStatus.Ready, FileName = "a", WorkPackageId = workPackageId });
            fx.Context.SaveChanges();
            fx.WorkPackageService.Setup(s => s.GetWorkPackageByIdAsync(workPackageId)).ReturnsAsync((WorkPackageDTO?)null);
            fx.ProjectService.Setup(s => s.GetUserInfoAsync(It.IsAny<Guid>())).ReturnsAsync((UserInfoDTO?)null);

            var result = await fx.Repository.GetAttachmentDetailsAsync(id);

            Assert.NotNull(result);
            Assert.Null(result!.WorkPackageTitle);
            Assert.Null(result.UploadedByUsername);
            Assert.Null(result.UploadedByRole);
        }

        [Fact]
        public async Task GetAttachmentDetailsAsync_ForNonexistentId_ReturnsNull()
        {
            var fx = new Fixture();

            var result = await fx.Repository.GetAttachmentDetailsAsync(Guid.NewGuid());

            Assert.Null(result);
        }
    }
}
