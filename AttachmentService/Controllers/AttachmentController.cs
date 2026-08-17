using AttachmentService.Data;
using AttachmentService.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace AttachmentService.Controllers
{
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

        // GET /attachments
        // GET /attachments?projectId={id}            -> project-level + all its workpackages'
        // GET /attachments?workPackageId={id}         -> just that one workpackage's
        [HttpGet]
        public ActionResult<IEnumerable<AttachmentDTO>> GetAttachments([FromQuery] Guid? projectId, [FromQuery] Guid? workPackageId)
        {
            return Ok(_attachmentRepository.GetAttachments(projectId, workPackageId));
        }

        // GET /tasks/{taskId}/attachments
        [HttpGet("~/tasks/{taskId:guid}/attachments")]
        public ActionResult<IEnumerable<AttachmentDTO>> GetAttachmentsForTask(Guid taskId)
        {
            return Ok(_attachmentRepository.GetAttachments(workPackageId: taskId));
        }

        // GET /attachments/{id}
        [HttpGet("{id:guid}", Name = "GetAttachmentById")]
        public ActionResult<AttachmentDTO> GetAttachmentById(Guid id)
        {
            var attachment = _attachmentRepository.GetAttachmentById(id);

            if (attachment is null)
            {
                return NotFound();
            }

            return Ok(attachment);
        }

        // GET /attachments/{id}/download
        [HttpGet("{id:guid}/download")]
        public IActionResult DownloadAttachment(Guid id)
        {
            var downloadUrl = _attachmentRepository.GetDownloadUrl(id);

            if (downloadUrl is null)
            {
                return NotFound();
            }

            return Redirect(downloadUrl);
        }

        // GET /attachments/{id}/details
        [HttpGet("{id:guid}/details")]
        public async Task<ActionResult<AttachmentDetailsDTO>> GetAttachmentDetails(Guid id)
        {
            var details = await _attachmentRepository.GetAttachmentDetailsAsync(id);

            if (details is null)
            {
                return NotFound();
            }

            return Ok(details);
        }

        // POST /attachments/upload
        [HttpPost("upload")]
        public ActionResult<AttachmentUploadResponseDTO> CreateAttachment(AttachmentCreationDTO attachmentDto)
        {
            if (!Request.Headers.TryGetValue(UserIdHeaderName, out var userIdHeader) ||
                !Guid.TryParse(userIdHeader, out var uploadedByUserId))
            {
                return BadRequest($"Missing or invalid {UserIdHeaderName} header - this is expected to be set by the API Gateway after authenticating the caller.");
            }

            var created = _attachmentRepository.CreateAttachment(attachmentDto, uploadedByUserId);

            return CreatedAtRoute("GetAttachmentById", new { id = created.Attachment.Id }, created);
        }

        [HttpPost("~/tasks/{taskId:guid}/attachments")]
        public ActionResult<AttachmentUploadResponseDTO> CreateAttachmentForTask(Guid taskId, AttachmentCreationDTO attachmentDto)
        {
            if (!Request.Headers.TryGetValue(UserIdHeaderName, out var userIdHeader) ||
                !Guid.TryParse(userIdHeader, out var uploadedByUserId))
            {
                return BadRequest($"Missing or invalid {UserIdHeaderName} header - this is expected to be set by the API Gateway after authenticating the caller.");
            }

            attachmentDto.WorkPackageId = taskId;

            var created = _attachmentRepository.CreateAttachment(attachmentDto, uploadedByUserId);

            return CreatedAtRoute("GetAttachmentById", new { id = created.Attachment.Id }, created);
        }

        // POST /attachments/confirm
        [HttpPost("confirm")]
        public async Task<ActionResult<AttachmentDTO>> ConfirmAttachment(AttachmentConfirmationDTO confirmationDto)
        {
            var result = await _attachmentRepository.ConfirmAttachmentAsync(confirmationDto);

            return result.Outcome switch
            {
                ConfirmAttachmentOutcome.Success => Ok(result.Attachment),
                ConfirmAttachmentOutcome.NotFound => NotFound(),
                ConfirmAttachmentOutcome.InvalidState => Conflict("Attachment is not awaiting upload confirmation."),
                ConfirmAttachmentOutcome.ObjectMissing => Conflict("No file was found in storage for this attachment yet."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        // PUT /attachments/{id}
        [HttpPut("{id:guid}")]
        public ActionResult<AttachmentDTO> UpdateAttachment(Guid id, AttachmentUpdateDTO attachmentDto)
        {
            var updated = _attachmentRepository.UpdateAttachment(id, attachmentDto);

            if (updated is null)
            {
                return NotFound();
            }

            return Ok(updated);
        }

        // DELETE /attachments/{attachmentId}
        [HttpDelete("{id:guid}")]
        public IActionResult DeleteAttachment(Guid id)
        {
            if (_attachmentRepository.GetAttachmentById(id) is null)
            {
                return NotFound();
            }

            _attachmentRepository.DeleteAttachment(id);

            return NoContent();
        }
    }
}
