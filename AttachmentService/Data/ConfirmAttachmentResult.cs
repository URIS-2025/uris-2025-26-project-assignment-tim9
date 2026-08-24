using AttachmentService.Models.DTO;

namespace AttachmentService.Data
{
    public record ConfirmAttachmentResult(ConfirmAttachmentOutcome Outcome, AttachmentDTO? Attachment);
}
