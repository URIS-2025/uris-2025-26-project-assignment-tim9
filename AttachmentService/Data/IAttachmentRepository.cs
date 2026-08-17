using AttachmentService.Models.DTO;

namespace AttachmentService.Data
{
    public interface IAttachmentRepository
    {
        IEnumerable<AttachmentDTO> GetAttachments(Guid? projectId = null, Guid? workPackageId = null);
        AttachmentDTO? GetAttachmentById(Guid id);
        string? GetDownloadUrl(Guid id);
        Task<AttachmentDetailsDTO?> GetAttachmentDetailsAsync(Guid id);
        AttachmentUploadResponseDTO CreateAttachment(AttachmentCreationDTO attachment, Guid uploadedByUserId);
        Task<ConfirmAttachmentResult> ConfirmAttachmentAsync(AttachmentConfirmationDTO confirmation);
        AttachmentDTO? UpdateAttachment(Guid id, AttachmentUpdateDTO attachment);
        void DeleteAttachment(Guid id);
    }
}
