using System.Net;
using System.Net.Http.Json;
using System.Text;
using AttachmentService.Models.DTO;
using AttachmentService.Models.Enums;

namespace AttachmentService.Tests.Integration
{
    /// <summary>
    /// Exercises the real HTTP pipeline (routing, model binding/validation, DI, controller,
    /// repository) against the real local MySQL and MinIO, plus fake HTTP stand-ins for
    /// WorkPackage/Project Service - the same infrastructure and scenarios verified by hand
    /// with curl throughout development, now automated. Requires a local MySQL reachable on
    /// localhost:3306 (root/root) and MinIO on localhost:9000 with an "attachments" bucket -
    /// same prerequisites as README.md.
    /// </summary>
    public class AttachmentApiIntegrationTests : IClassFixture<AttachmentApiFixture>
    {
        private readonly AttachmentApiFixture _fx;

        public AttachmentApiIntegrationTests(AttachmentApiFixture fixture)
        {
            _fx = fixture;
        }

        private static HttpRequestMessage CreateRequest(HttpMethod method, string url, Guid? userId = null, object? body = null)
        {
            var request = new HttpRequestMessage(method, url);
            if (userId is not null)
            {
                request.Headers.Add("X-User-Id", userId.ToString());
            }
            if (body is not null)
            {
                request.Content = JsonContent.Create(body);
            }
            return request;
        }

        [Fact]
        public async Task FullLifecycle_UploadConfirmDownloadUpdateDelete_WorksEndToEnd()
        {
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            const string content = "integration test file content - full lifecycle";

            // 1) Initiate upload
            var createRequest = CreateRequest(HttpMethod.Post, "/attachments/upload", userId, new
            {
                originalFileName = "lifecycle.txt",
                contentType = "text/plain",
                fileSize = Encoding.UTF8.GetByteCount(content),
                projectId
            });
            var createResponse = await _fx.Client.SendAsync(createRequest);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var created = await createResponse.Content.ReadFromJsonAsync<AttachmentUploadResponseDTO>();
            Assert.NotNull(created);
            Assert.Equal(AttachmentStatus.Uploading, created!.Attachment.Status);

            // 2) Upload real bytes straight to MinIO, exactly as a real client would.
            // NOTE: StringContent(text, encoding, mediaType) appends "; charset=utf-8" to
            // Content-Type - that doesn't match the exact "text/plain" signed into the
            // presigned URL's SignedHeaders, and MinIO rejects the mismatch as 403 Forbidden.
            // Found by actually running this, not by inspection. ByteArrayContent with an
            // explicit bare Content-Type avoids it.
            var uploadContent = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
            uploadContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
            var putResponse = await _fx.StorageClient.PutAsync(created.UploadUrl, uploadContent);
            Assert.True(putResponse.IsSuccessStatusCode, $"PUT to storage failed: {putResponse.StatusCode}");

            // 3) Confirm
            var confirmRequest = CreateRequest(HttpMethod.Post, "/attachments/confirm", body: new { attachmentId = created.Attachment.Id });
            var confirmResponse = await _fx.Client.SendAsync(confirmRequest);
            Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);
            var confirmed = await confirmResponse.Content.ReadFromJsonAsync<AttachmentDTO>();
            Assert.Equal(AttachmentStatus.Ready, confirmed!.Status);

            // 4) Download - the API 302s to a presigned URL, following it must return the exact bytes
            var downloadResponse = await _fx.Client.GetAsync($"/attachments/{created.Attachment.Id}/download");
            Assert.Equal(HttpStatusCode.Redirect, downloadResponse.StatusCode);
            var downloadUrl = downloadResponse.Headers.Location!.ToString();

            var actualFile = await _fx.StorageClient.GetAsync(downloadUrl);
            var actualContent = await actualFile.Content.ReadAsStringAsync();
            Assert.Equal(content, actualContent);

            // 5) Update - only description, filename should survive untouched
            var updateRequest = CreateRequest(HttpMethod.Put, $"/attachments/{created.Attachment.Id}", body: new { description = "updated via integration test" });
            var updateResponse = await _fx.Client.SendAsync(updateRequest);
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            var updated = await updateResponse.Content.ReadFromJsonAsync<AttachmentDTO>();
            Assert.Equal("updated via integration test", updated!.Description);
            Assert.Equal(confirmed.FileName, updated.FileName);

            // 6) Delete, then confirm it's really gone
            var deleteResponse = await _fx.Client.DeleteAsync($"/attachments/{created.Attachment.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getAfterDelete = await _fx.Client.GetAsync($"/attachments/{created.Attachment.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
        }

        [Fact]
        public async Task CreateAttachment_WithoutUserIdHeader_ReturnsBadRequest()
        {
            var request = CreateRequest(HttpMethod.Post, "/attachments/upload", userId: null, body: new
            {
                originalFileName = "x.txt",
                contentType = "text/plain",
                fileSize = 5,
                projectId = Guid.NewGuid()
            });

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateAttachment_WithDisallowedContentType_ReturnsBadRequestFromRealValidationPipeline()
        {
            var request = CreateRequest(HttpMethod.Post, "/attachments/upload", Guid.NewGuid(), new
            {
                originalFileName = "virus.exe",
                contentType = "application/x-msdownload",
                fileSize = 5,
                projectId = Guid.NewGuid()
            });

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("not an allowed content type", body);
        }

        [Fact]
        public async Task ConfirmAttachment_WithoutUploadingToStorageFirst_ReturnsConflict()
        {
            // Create, but deliberately skip the PUT-to-storage step.
            var createRequest = CreateRequest(HttpMethod.Post, "/attachments/upload", Guid.NewGuid(), new
            {
                originalFileName = "never-uploaded.txt",
                contentType = "text/plain",
                fileSize = 5,
                projectId = Guid.NewGuid()
            });
            var createResponse = await _fx.Client.SendAsync(createRequest);
            var created = await createResponse.Content.ReadFromJsonAsync<AttachmentUploadResponseDTO>();

            var confirmRequest = CreateRequest(HttpMethod.Post, "/attachments/confirm", body: new { attachmentId = created!.Attachment.Id });
            var confirmResponse = await _fx.Client.SendAsync(confirmRequest);

            Assert.Equal(HttpStatusCode.Conflict, confirmResponse.StatusCode);
        }

        [Fact]
        public async Task ConfirmAttachment_ForNonexistentId_ReturnsNotFound()
        {
            var request = CreateRequest(HttpMethod.Post, "/attachments/confirm", body: new { attachmentId = Guid.NewGuid() });

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DownloadAttachment_BeforeConfirm_ReturnsNotFound()
        {
            var createRequest = CreateRequest(HttpMethod.Post, "/attachments/upload", Guid.NewGuid(), new
            {
                originalFileName = "unconfirmed.txt",
                contentType = "text/plain",
                fileSize = 5,
                projectId = Guid.NewGuid()
            });
            var createResponse = await _fx.Client.SendAsync(createRequest);
            var created = await createResponse.Content.ReadFromJsonAsync<AttachmentUploadResponseDTO>();

            var downloadResponse = await _fx.Client.GetAsync($"/attachments/{created!.Attachment.Id}/download");

            Assert.Equal(HttpStatusCode.NotFound, downloadResponse.StatusCode);
        }

        [Fact]
        public async Task TaskScopedUpload_SetsWorkPackageIdFromRouteAndIsListedUnderThatTask()
        {
            var taskId = Guid.NewGuid();
            var request = CreateRequest(HttpMethod.Post, $"/tasks/{taskId}/attachments", Guid.NewGuid(), new
            {
                originalFileName = "task-file.txt",
                contentType = "text/plain",
                fileSize = 5,
                projectId = Guid.NewGuid()
                // Deliberately no workPackageId in the body - the route must win.
            });

            var response = await _fx.Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<AttachmentUploadResponseDTO>();
            Assert.Equal(taskId, created!.Attachment.WorkPackageId);

            var listResponse = await _fx.Client.GetAsync($"/tasks/{taskId}/attachments");
            var list = await listResponse.Content.ReadFromJsonAsync<List<AttachmentDTO>>();
            Assert.Contains(list!, a => a.Id == created.Attachment.Id);
        }

        [Fact]
        public async Task GetAttachments_FilteredByProjectId_IncludesWorkPackageLevelAttachments()
        {
            var projectId = Guid.NewGuid();
            var workPackageId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            async Task Create(Guid? wpId)
            {
                var req = CreateRequest(HttpMethod.Post, "/attachments/upload", userId, new
                {
                    originalFileName = "f.txt",
                    contentType = "text/plain",
                    fileSize = 5,
                    projectId,
                    workPackageId = wpId
                });
                var resp = await _fx.Client.SendAsync(req);
                Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
            }

            await Create(null);
            await Create(workPackageId);

            var response = await _fx.Client.GetAsync($"/attachments?projectId={projectId}");
            var list = await response.Content.ReadFromJsonAsync<List<AttachmentDTO>>();

            Assert.Equal(2, list!.Count);
        }

        [Fact]
        public async Task GetAttachments_FilteredByWorkPackageId_ReturnsOnlyThatWorkPackage()
        {
            var projectId = Guid.NewGuid();
            var workPackageId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var withWorkPackage = CreateRequest(HttpMethod.Post, "/attachments/upload", userId, new
            {
                originalFileName = "in-wp.txt",
                contentType = "text/plain",
                fileSize = 5,
                projectId,
                workPackageId
            });
            await _fx.Client.SendAsync(withWorkPackage);

            var withoutWorkPackage = CreateRequest(HttpMethod.Post, "/attachments/upload", userId, new
            {
                originalFileName = "project-level.txt",
                contentType = "text/plain",
                fileSize = 5,
                projectId
            });
            await _fx.Client.SendAsync(withoutWorkPackage);

            var response = await _fx.Client.GetAsync($"/attachments?workPackageId={workPackageId}");
            var list = await response.Content.ReadFromJsonAsync<List<AttachmentDTO>>();

            Assert.Single(list!);
            Assert.Equal("in-wp.txt", list![0].OriginalFileName);
        }

        [Fact]
        public async Task GetAttachmentDetails_EnrichesWithRealWorkPackageAndUploaderInfo()
        {
            // Uses the fixture's known ids, which the fake WorkPackage/Project servers
            // recognize - proves the real Controller -> Repository -> IWorkPackageService/
            // IProjectService -> HttpClient -> fake server -> JSON deserialize chain end to end.
            var createRequest = CreateRequest(HttpMethod.Post, "/attachments/upload", _fx.KnownUploaderId, new
            {
                originalFileName = "enriched.txt",
                contentType = "text/plain",
                fileSize = 5,
                projectId = Guid.NewGuid(),
                workPackageId = _fx.KnownWorkPackageId
            });
            var createResponse = await _fx.Client.SendAsync(createRequest);
            var created = await createResponse.Content.ReadFromJsonAsync<AttachmentUploadResponseDTO>();

            var detailsResponse = await _fx.Client.GetAsync($"/attachments/{created!.Attachment.Id}/details");
            var details = await detailsResponse.Content.ReadFromJsonAsync<AttachmentDetailsDTO>();

            Assert.Equal(AttachmentApiFixture.KnownWorkPackageTitle, details!.WorkPackageTitle);
            Assert.Equal(AttachmentApiFixture.KnownUploaderUsername, details.UploadedByUsername);
            Assert.Equal(AttachmentApiFixture.KnownUploaderRole, details.UploadedByRole);
        }

        [Fact]
        public async Task GetAttachmentDetails_ForUnknownWorkPackageAndUploader_DegradesGracefully()
        {
            // Ids the fake servers don't recognize -> real 404s from those servers -> should
            // degrade to null fields rather than fail the whole request.
            var createRequest = CreateRequest(HttpMethod.Post, "/attachments/upload", Guid.NewGuid(), new
            {
                originalFileName = "unrecognized.txt",
                contentType = "text/plain",
                fileSize = 5,
                projectId = Guid.NewGuid(),
                workPackageId = Guid.NewGuid()
            });
            var createResponse = await _fx.Client.SendAsync(createRequest);
            var created = await createResponse.Content.ReadFromJsonAsync<AttachmentUploadResponseDTO>();

            var detailsResponse = await _fx.Client.GetAsync($"/attachments/{created!.Attachment.Id}/details");
            var details = await detailsResponse.Content.ReadFromJsonAsync<AttachmentDetailsDTO>();

            Assert.Equal(HttpStatusCode.OK, detailsResponse.StatusCode);
            Assert.Null(details!.WorkPackageTitle);
            Assert.Null(details.UploadedByUsername);
        }
    }
}
