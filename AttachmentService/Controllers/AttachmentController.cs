using AttachmentService.Data;
using AttachmentService.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace AttachmentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttachmentController : ControllerBase
    {
        private readonly IAttachmentRepository _attachmentRepository;

        public AttachmentController(IAttachmentRepository attachmentRepository)
        {
            _attachmentRepository = attachmentRepository;
        }

        // GET api/attachment
        [HttpGet]
        public ActionResult<IEnumerable<AttachmentDTO>> GetAttachments()
        {
            return Ok(_attachmentRepository.GetAttachments());
        }

        // GET api/attachment/{id}
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

        // POST api/attachment
        [HttpPost]
        public ActionResult<AttachmentDTO> CreateAttachment(AttachmentCreationDTO attachmentDto)
        {
            var created = _attachmentRepository.CreateAttachment(attachmentDto);

            return CreatedAtRoute("GetAttachmentById", new { id = created.Id }, created);
        }

        // POST api/attachment/confirm
        [HttpPost("confirm")]
        public ActionResult<AttachmentDTO> ConfirmAttachment(AttachmentConfirmationDTO confirmationDto)
        {
            var confirmed = _attachmentRepository.ConfirmAttachment(confirmationDto);

            if (confirmed is null)
            {
                return NotFound();
            }

            return Ok(confirmed);
        }

        // PUT api/attachment/{id}
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

        // DELETE api/attachment/{id}
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
