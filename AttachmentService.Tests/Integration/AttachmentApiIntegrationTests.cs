using System.Net;
using System.Net.Http.Json;
using System.Text;
using AttachmentService.Models.DTO;
using AttachmentService.Models.Enums;

namespace AttachmentService.Tests.Integration
{
    /// <summary>
    /// Exercises the real HTTP pipeline (JWT authentication, routing, model binding/validation,
    /// DI, controller, repository) against the real local MySQL and MinIO, plus fake HTTP
    /// stand-ins for WorkPackage/Project/User Service - the same infrastructure and scenarios
    /// verified by hand with curl/Swagger throughout development, now automated. Requires a
    /// local MySQL reachable on localhost:3306 (root/root) and MinIO on localhost:9000 with an
    /// "attachments" bucket for the tests that actually touch storage (upload/confirm/download);
    /// the rest (authorization, existence validation, identity enrichment) only need MySQL.
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

        private async Task<AttachmentUploadResponseDTO> CreateAttachmentAsync(Guid uploaderId, Guid projectId, Guid? taskId = null, string fileName = "f.txt")
        {
            var request = CreateRequest(HttpMethod.Post, "/attachments/upload", uploaderId, new
            {
                originalFileName = fileName,
                contentType = "text/plain",
                fileSize = 5,
                projectId,
                taskId
            });
            var response = await _fx.Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<AttachmentUploadResponseDTO>())!;
        }

        // ---------- Full lifecycle (requires MinIO) ----------

        [Fact]
        public async Task FullLifecycle_UploadConfirmDownloadUpdateDelete_WorksEndToEnd()
        {
            var projectId = Guid.NewGuid();
            const string content = "integration test file content - full lifecycle";

            // 1) Initiate upload
            var createRequest = CreateRequest(HttpMethod.Post, "/attachments/upload", _fx.MemberUserId, new
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
            Assert.Equal(AttachmentApiFixture.MemberUsername, created.UploadedByUsername);
            Assert.Equal(AttachmentApiFixture.MemberRole, created.UploadedByRole);

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

            // 3) Confirm - by the uploader themselves
            var confirmRequest = CreateRequest(HttpMethod.Post, "/attachments/confirm", _fx.MemberUserId, new { attachmentId = created.Attachment.Id });
            var confirmResponse = await _fx.Client.SendAsync(confirmRequest);
            Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);
            var confirmed = await confirmResponse.Content.ReadFromJsonAsync<AttachmentDTO>();
            Assert.Equal(AttachmentStatus.Ready, confirmed!.Status);

            // 4) Download - the API 302s to a presigned URL, following it must return the exact bytes
            var downloadRequest = CreateRequest(HttpMethod.Get, $"/attachments/{created.Attachment.Id}/download", _fx.MemberUserId);
            var downloadResponse = await _fx.Client.SendAsync(downloadRequest);
            Assert.Equal(HttpStatusCode.Redirect, downloadResponse.StatusCode);
            var downloadUrl = downloadResponse.Headers.Location!.ToString();

            var actualFile = await _fx.StorageClient.GetAsync(downloadUrl);
            var actualContent = await actualFile.Content.ReadAsStringAsync();
            Assert.Equal(content, actualContent);

            // 5) Update - only description, filename should survive untouched. Owner-only.
            var updateRequest = CreateRequest(HttpMethod.Put, $"/attachments/{created.Attachment.Id}", _fx.MemberUserId, new { description = "updated via integration test" });
            var updateResponse = await _fx.Client.SendAsync(updateRequest);
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            var updated = await updateResponse.Content.ReadFromJsonAsync<AttachmentDTO>();
            Assert.Equal("updated via integration test", updated!.Description);
            Assert.Equal(confirmed.FileName, updated.FileName);

            // 6) Delete, then confirm it's really gone. Owner-only.
            var deleteRequest = CreateRequest(HttpMethod.Delete, $"/attachments/{created.Attachment.Id}", _fx.MemberUserId);
            var deleteResponse = await _fx.Client.SendAsync(deleteRequest);
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getAfterDelete = CreateRequest(HttpMethod.Get, $"/attachments/{created.Attachment.Id}", _fx.MemberUserId);
            var getAfterDeleteResponse = await _fx.Client.SendAsync(getAfterDelete);
            Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
        }

        // ---------- Create: validation & existence ----------

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
            var request = CreateRequest(HttpMethod.Post, "/attachments/upload", _fx.MemberUserId, new
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
        public async Task CreateAttachment_ForNonexistentProject_ReturnsBadRequest()
        {
            var request = CreateRequest(HttpMethod.Post, "/attachments/upload", _fx.MemberUserId, new
            {
                originalFileName = "x.txt",
                contentType = "text/plain",
                fileSize = 5,
                projectId = _fx.MissingProjectId
            });

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains(_fx.MissingProjectId.ToString(), body);
        }

        [Fact]
        public async Task CreateAttachment_ForNonexistentTask_ReturnsBadRequest()
        {
            var request = CreateRequest(HttpMethod.Post, "/attachments/upload", _fx.MemberUserId, new
            {
                originalFileName = "x.txt",
                contentType = "text/plain",
                fileSize = 5,
                projectId = Guid.NewGuid(),
                taskId = _fx.MissingTaskId
            });

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains(_fx.MissingTaskId.ToString(), body);
        }

        // ---------- Create: authorization ----------

        [Fact]
        public async Task CreateAttachment_ByClientRole_ReturnsForbidden()
        {
            var request = CreateRequest(HttpMethod.Post, "/attachments/upload", _fx.ClientUserId, new
            {
                originalFileName = "x.txt",
                contentType = "text/plain",
                fileSize = 5,
                projectId = Guid.NewGuid()
            });

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateAttachment_ByNonMember_ReturnsForbidden()
        {
            var request = CreateRequest(HttpMethod.Post, "/attachments/upload", _fx.MemberUserId, new
            {
                originalFileName = "x.txt",
                contentType = "text/plain",
                fileSize = 5,
                projectId = _fx.NonMemberProjectId
            });

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData(nameof(AttachmentApiFixture.MemberUserId))]
        [InlineData(nameof(AttachmentApiFixture.ProjectManagerUserId))]
        public async Task CreateAttachment_ByUploaderRole_Succeeds(string userIdProperty)
        {
            var uploaderId = userIdProperty == nameof(AttachmentApiFixture.MemberUserId) ? _fx.MemberUserId : _fx.ProjectManagerUserId;

            var created = await CreateAttachmentAsync(uploaderId, Guid.NewGuid());

            Assert.Equal(AttachmentStatus.Uploading, created.Attachment.Status);
        }

        [Fact]
        public async Task CreateAttachment_ByAdmin_BypassesMembershipAndRole()
        {
            // NonMemberProjectId has zero members - only an Admin can create here.
            var created = await CreateAttachmentAsync(_fx.AdminUserId, _fx.NonMemberProjectId);

            Assert.Equal(AttachmentStatus.Uploading, created.Attachment.Status);
        }

        [Fact]
        public async Task TaskScopedUpload_SetsTaskIdFromRouteAndIsListedUnderThatTask()
        {
            var request = CreateRequest(HttpMethod.Post, $"/tasks/{_fx.KnownTaskId}/attachments", _fx.MemberUserId, new
            {
                originalFileName = "task-file.txt",
                contentType = "text/plain",
                fileSize = 5,
                projectId = Guid.NewGuid()
                // Deliberately no taskId in the body - the route must win.
            });

            var response = await _fx.Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<AttachmentUploadResponseDTO>();
            Assert.Equal(_fx.KnownTaskId, created!.Attachment.TaskId);

            var listRequest = CreateRequest(HttpMethod.Get, $"/tasks/{_fx.KnownTaskId}/attachments", _fx.MemberUserId);
            var listResponse = await _fx.Client.SendAsync(listRequest);
            var list = await listResponse.Content.ReadFromJsonAsync<List<AttachmentDTO>>();
            Assert.Contains(list!, a => a.Id == created.Attachment.Id);
        }

        // ---------- Confirm (needs MinIO for the success path; not for Forbidden/NotFound) ----------

        [Fact]
        public async Task ConfirmAttachment_WithoutUploadingToStorageFirst_ReturnsConflict()
        {
            var created = await CreateAttachmentAsync(_fx.MemberUserId, Guid.NewGuid());

            var confirmRequest = CreateRequest(HttpMethod.Post, "/attachments/confirm", _fx.MemberUserId, new { attachmentId = created.Attachment.Id });
            var confirmResponse = await _fx.Client.SendAsync(confirmRequest);

            Assert.Equal(HttpStatusCode.Conflict, confirmResponse.StatusCode);
        }

        [Fact]
        public async Task ConfirmAttachment_ForNonexistentId_ReturnsNotFound()
        {
            var request = CreateRequest(HttpMethod.Post, "/attachments/confirm", _fx.MemberUserId, new { attachmentId = Guid.NewGuid() });

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ConfirmAttachment_ByNonOwnerNonAdmin_ReturnsForbidden()
        {
            var created = await CreateAttachmentAsync(_fx.MemberUserId, Guid.NewGuid());

            var confirmRequest = CreateRequest(HttpMethod.Post, "/attachments/confirm", _fx.ProjectManagerUserId, new { attachmentId = created.Attachment.Id });
            var confirmResponse = await _fx.Client.SendAsync(confirmRequest);

            Assert.Equal(HttpStatusCode.Forbidden, confirmResponse.StatusCode);
        }

        [Fact]
        public async Task DownloadAttachment_BeforeConfirm_ReturnsNotFound()
        {
            var created = await CreateAttachmentAsync(_fx.MemberUserId, Guid.NewGuid());

            var downloadRequest = CreateRequest(HttpMethod.Get, $"/attachments/{created.Attachment.Id}/download", _fx.MemberUserId);
            var downloadResponse = await _fx.Client.SendAsync(downloadRequest);

            Assert.Equal(HttpStatusCode.NotFound, downloadResponse.StatusCode);
        }

        // ---------- Listing: filters ----------

        [Fact]
        public async Task GetAttachments_FilteredByProjectId_IncludesTaskLevelAttachments()
        {
            var projectId = Guid.NewGuid();

            await CreateAttachmentAsync(_fx.MemberUserId, projectId, taskId: null, fileName: "project-level.txt");
            await CreateAttachmentAsync(_fx.MemberUserId, projectId, taskId: _fx.KnownTaskId, fileName: "task-level.txt");

            var request = CreateRequest(HttpMethod.Get, $"/attachments?projectId={projectId}", _fx.MemberUserId);
            var response = await _fx.Client.SendAsync(request);
            var list = await response.Content.ReadFromJsonAsync<List<AttachmentDTO>>();

            Assert.Equal(2, list!.Count);
        }

        [Fact]
        public async Task GetAttachments_FilteredByTaskId_ReturnsOnlyThatTask()
        {
            // KnownTaskId is a fixture-wide reserved id shared with other tests in this class
            // (IClassFixture keeps one database for the whole class), so this can't assert an
            // exact count - only that every returned row really belongs to that task, that our
            // own upload is among them, and that a project-level upload from this same test
            // never leaks in.
            var projectId = Guid.NewGuid();

            var inTask = await CreateAttachmentAsync(_fx.MemberUserId, projectId, taskId: _fx.KnownTaskId, fileName: "in-task.txt");
            var projectLevel = await CreateAttachmentAsync(_fx.MemberUserId, projectId, taskId: null, fileName: "project-level.txt");

            var request = CreateRequest(HttpMethod.Get, $"/attachments?taskId={_fx.KnownTaskId}", _fx.MemberUserId);
            var response = await _fx.Client.SendAsync(request);
            var list = await response.Content.ReadFromJsonAsync<List<AttachmentDTO>>();

            Assert.All(list!, a => Assert.Equal(_fx.KnownTaskId, a.TaskId));
            Assert.Contains(list!, a => a.Id == inTask.Attachment.Id);
            Assert.DoesNotContain(list!, a => a.Id == projectLevel.Attachment.Id);
        }

        [Fact]
        public async Task GetAttachments_WithoutFilter_RequiresAdmin()
        {
            var request = CreateRequest(HttpMethod.Get, "/attachments", _fx.MemberUserId);

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAttachments_WithoutFilter_AsAdmin_Succeeds()
        {
            var request = CreateRequest(HttpMethod.Get, "/attachments", _fx.AdminUserId);

            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ---------- Listing/viewing: authorization ----------

        [Fact]
        public async Task GetAttachments_ByNonMember_ReturnsForbidden()
        {
            // Only an Admin can create in the member-less project; a non-admin, non-member
            // then tries to list it.
            await CreateAttachmentAsync(_fx.AdminUserId, _fx.NonMemberProjectId);

            var request = CreateRequest(HttpMethod.Get, $"/attachments?projectId={_fx.NonMemberProjectId}", _fx.MemberUserId);
            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetAttachments_ByClientRole_CanStillView()
        {
            var projectId = Guid.NewGuid();
            await CreateAttachmentAsync(_fx.MemberUserId, projectId);

            var request = CreateRequest(HttpMethod.Get, $"/attachments?projectId={projectId}", _fx.ClientUserId);
            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ---------- Details: enrichment ----------

        [Fact]
        public async Task GetAttachmentDetails_EnrichesWithRealTaskAndUploaderInfo()
        {
            var created = await CreateAttachmentAsync(_fx.MemberUserId, Guid.NewGuid(), taskId: _fx.KnownTaskId);

            var detailsRequest = CreateRequest(HttpMethod.Get, $"/attachments/{created.Attachment.Id}/details", _fx.MemberUserId);
            var detailsResponse = await _fx.Client.SendAsync(detailsRequest);
            var details = await detailsResponse.Content.ReadFromJsonAsync<AttachmentDetailsDTO>();

            Assert.Equal(AttachmentApiFixture.KnownTaskTitle, details!.TaskTitle);
            Assert.Equal(AttachmentApiFixture.MemberUsername, details.UploadedByUsername);
            Assert.Equal(AttachmentApiFixture.MemberRole, details.UploadedByRole);
        }

        [Fact]
        public async Task GetAttachmentDetails_ForUploaderUnknownToUserService_DegradesUploaderFieldsToNull()
        {
            // UnknownToUserServiceMemberId is a real project member (so creation succeeds) but
            // the fake User server doesn't recognize it - identity enrichment should degrade
            // to null instead of failing the whole request.
            var created = await CreateAttachmentAsync(_fx.UnknownToUserServiceMemberId, Guid.NewGuid());

            var detailsRequest = CreateRequest(HttpMethod.Get, $"/attachments/{created.Attachment.Id}/details", _fx.MemberUserId);
            var detailsResponse = await _fx.Client.SendAsync(detailsRequest);
            var details = await detailsResponse.Content.ReadFromJsonAsync<AttachmentDetailsDTO>();

            Assert.Equal(HttpStatusCode.OK, detailsResponse.StatusCode);
            Assert.Null(details!.UploadedByUsername);
            Assert.Null(details.UploadedByRole);
        }

        // ---------- Update / Delete: ownership ----------

        [Fact]
        public async Task UpdateAttachment_ByOwner_Succeeds()
        {
            var created = await CreateAttachmentAsync(_fx.MemberUserId, Guid.NewGuid());

            var request = CreateRequest(HttpMethod.Put, $"/attachments/{created.Attachment.Id}", _fx.MemberUserId, new { description = "mine to edit" });
            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UpdateAttachment_ByNonOwnerNonAdmin_ReturnsForbidden()
        {
            var created = await CreateAttachmentAsync(_fx.MemberUserId, Guid.NewGuid());

            var request = CreateRequest(HttpMethod.Put, $"/attachments/{created.Attachment.Id}", _fx.ProjectManagerUserId, new { description = "not yours" });
            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UpdateAttachment_ByAdmin_Succeeds()
        {
            var created = await CreateAttachmentAsync(_fx.MemberUserId, Guid.NewGuid());

            var request = CreateRequest(HttpMethod.Put, $"/attachments/{created.Attachment.Id}", _fx.AdminUserId, new { description = "admin override" });
            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DeleteAttachment_ByNonOwnerNonAdmin_ReturnsForbidden()
        {
            var created = await CreateAttachmentAsync(_fx.MemberUserId, Guid.NewGuid());

            var request = CreateRequest(HttpMethod.Delete, $"/attachments/{created.Attachment.Id}", _fx.ProjectManagerUserId);
            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteAttachment_ByAdmin_Succeeds()
        {
            var created = await CreateAttachmentAsync(_fx.MemberUserId, Guid.NewGuid());

            var request = CreateRequest(HttpMethod.Delete, $"/attachments/{created.Attachment.Id}", _fx.AdminUserId);
            var response = await _fx.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
