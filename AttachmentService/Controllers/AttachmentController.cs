using AttachmentService.Data;
using AttachmentService.Exceptions;
using AttachmentService.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttachmentService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("attachments")]
    public class AttachmentController : ControllerBase
    {
        private const string UserIdHeaderName = "X-User-Id";

        private readonly IAttachmentRepository _attachmentRepository;

        public AttachmentController(IAttachmentRepository attachmentRepository)
        {
            _attachmentRepository = attachmentRepository;
        }

        private string? GetBearerToken()
        {
            var authHeader = Request.Headers.Authorization.ToString();
            return authHeader.StartsWith("Bearer ") ? authHeader["Bearer ".Length..] : null;
        }

        private bool TryGetActingUserId(out Guid userId)
        {
            userId = Guid.Empty;
            return Request.Headers.TryGetValue(UserIdHeaderName, out var userIdHeader) &&
                   Guid.TryParse(userIdHeader, out userId);
        }

        // GET /attachments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AttachmentDTO>>> GetAttachments([FromQuery] Guid? projectId, [FromQuery] Guid? taskId)
        {
            if (!TryGetActingUserId(out var actingUserId))
            {
                return BadRequest($"Missing or invalid {UserIdHeaderName} header - this is expected to be set by the API Gateway after authenticating the caller.");
            }

            try
            {
                return Ok(await _attachmentRepository.GetAttachmentsAsync(projectId, taskId, actingUserId, GetBearerToken()));
            }
            catch (ProjectContextRequiredException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UserNotProjectMemberException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
        }

        // GET /tasks/{taskId}/attachments
        [HttpGet("~/tasks/{taskId:guid}/attachments")]
        public Task<ActionResult<IEnumerable<AttachmentDTO>>> GetAttachmentsForTask(Guid taskId)
        {
            return GetAttachments(projectId: null, taskId: taskId);
        }

        // GET /attachments/{id}
        [HttpGet("{id:guid}", Name = "GetAttachmentById")]
        public async Task<ActionResult<AttachmentDTO>> GetAttachmentById(Guid id)
        {
            if (!TryGetActingUserId(out var actingUserId))
            {
                return BadRequest($"Missing or invalid {UserIdHeaderName} header - this is expected to be set by the API Gateway after authenticating the caller.");
            }

            try
            {
                var attachment = await _attachmentRepository.GetAttachmentByIdAsync(id, actingUserId, GetBearerToken());

                if (attachment is null)
                {
                    return NotFound();
                }

                return Ok(attachment);
            }
            catch (UserNotProjectMemberException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
        }

        // GET /attachments/{id}/download
        [HttpGet("{id:guid}/download")]
        public async Task<IActionResult> DownloadAttachment(Guid id)
        {
            if (!TryGetActingUserId(out var actingUserId))
            {
                return BadRequest($"Missing or invalid {UserIdHeaderName} header - this is expected to be set by the API Gateway after authenticating the caller.");
            }

            try
            {
                var downloadUrl = await _attachmentRepository.GetDownloadUrlAsync(id, actingUserId, GetBearerToken());

                if (downloadUrl is null)
                {
                    return NotFound();
                }

                return Redirect(downloadUrl);
            }
            catch (UserNotProjectMemberException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
        }

        // GET /attachments/{id}/details
        [HttpGet("{id:guid}/details")]
        public async Task<ActionResult<AttachmentDetailsDTO>> GetAttachmentDetails(Guid id)
        {
            if (!TryGetActingUserId(out var actingUserId))
            {
                return BadRequest($"Missing or invalid {UserIdHeaderName} header - this is expected to be set by the API Gateway after authenticating the caller.");
            }

            try
            {
                var details = await _attachmentRepository.GetAttachmentDetailsAsync(id, actingUserId, GetBearerToken());

                if (details is null)
                {
                    return NotFound();
                }

                return Ok(details);
            }
            catch (UserNotProjectMemberException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
        }

        // POST /attachments/upload
        [HttpPost("upload")]
        public async Task<ActionResult<AttachmentUploadResponseDTO>> CreateAttachment(AttachmentCreationDTO attachmentDto)
        {
            if (!TryGetActingUserId(out var uploadedByUserId))
            {
                return BadRequest($"Missing or invalid {UserIdHeaderName} header - this is expected to be set by the API Gateway after authenticating the caller.");
            }

            try
            {
                var created = await _attachmentRepository.CreateAttachmentAsync(attachmentDto, uploadedByUserId, GetBearerToken());
                return CreatedAtRoute("GetAttachmentById", new { id = created.Attachment.Id }, created);
            }
            catch (ProjectNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (TaskNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UserNotProjectMemberException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (RoleCannotUploadAttachmentsException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
        }

        [HttpPost("~/tasks/{taskId:guid}/attachments")]
        public Task<ActionResult<AttachmentUploadResponseDTO>> CreateAttachmentForTask(Guid taskId, AttachmentCreationDTO attachmentDto)
        {
            attachmentDto.TaskId = taskId;
            return CreateAttachment(attachmentDto);
        }

        // POST /attachments/confirm
        [HttpPost("confirm")]
        public async Task<ActionResult<AttachmentDTO>> ConfirmAttachment(AttachmentConfirmationDTO confirmationDto)
        {
            if (!TryGetActingUserId(out var actingUserId))
            {
                return BadRequest($"Missing or invalid {UserIdHeaderName} header - this is expected to be set by the API Gateway after authenticating the caller.");
            }

            try
            {
                var result = await _attachmentRepository.ConfirmAttachmentAsync(confirmationDto, actingUserId, GetBearerToken());

                return result.Outcome switch
                {
                    ConfirmAttachmentOutcome.Success => Ok(result.Attachment),
                    ConfirmAttachmentOutcome.NotFound => NotFound(),
                    ConfirmAttachmentOutcome.Forbidden => StatusCode(StatusCodes.Status403Forbidden, "Only the uploader or an Admin can confirm this attachment."),
                    ConfirmAttachmentOutcome.InvalidState => Conflict("Attachment is not awaiting upload confirmation."),
                    ConfirmAttachmentOutcome.ObjectMissing => Conflict("No file was found in storage for this attachment yet."),
                    _ => StatusCode(StatusCodes.Status500InternalServerError)
                };
            }
            catch (StorageUnavailableException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
            }
        }

        // PUT /attachments/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<AttachmentDTO>> UpdateAttachment(Guid id, AttachmentUpdateDTO attachmentDto)
        {
            if (!TryGetActingUserId(out var actingUserId))
            {
                return BadRequest($"Missing or invalid {UserIdHeaderName} header - this is expected to be set by the API Gateway after authenticating the caller.");
            }

            try
            {
                var updated = await _attachmentRepository.UpdateAttachmentAsync(id, attachmentDto, actingUserId, GetBearerToken());

                if (updated is null)
                {
                    return NotFound();
                }

                return Ok(updated);
            }
            catch (NotAttachmentOwnerException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
        }

        // DELETE /attachments/{attachmentId}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAttachment(Guid id)
        {
            if (!TryGetActingUserId(out var actingUserId))
            {
                return BadRequest($"Missing or invalid {UserIdHeaderName} header - this is expected to be set by the API Gateway after authenticating the caller.");
            }

            try
            {
                var deleted = await _attachmentRepository.DeleteAttachmentAsync(id, actingUserId, GetBearerToken());

                if (!deleted)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (NotAttachmentOwnerException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
        }
    }
}
