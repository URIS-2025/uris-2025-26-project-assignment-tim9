using AttachmentService.Models.DTO;

namespace AttachmentService.Data
{
    public interface IAttachmentRepository
    {
        IEnumerable<AttachmentDTO> GetAttachments();
        AttachmentDTO? GetAttachmentById(Guid id);
        AttachmentDTO CreateAttachment(AttachmentCreationDTO attachment);
        AttachmentDTO? ConfirmAttachment(AttachmentConfirmationDTO confirmation);
        AttachmentDTO? UpdateAttachment(Guid id, AttachmentUpdateDTO attachment);
        void DeleteAttachment(Guid id);
        bool SaveChanges();
    }
}
