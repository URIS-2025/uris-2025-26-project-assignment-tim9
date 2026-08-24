using AttachmentService.Models.DTO;

namespace AttachmentService.Data
{
    public interface IAttachmentRepository
    {
        Task<IEnumerable<AttachmentDTO>> GetAttachmentsAsync(Guid? projectId, Guid? taskId, Guid actingUserId, string? bearerToken);
        Task<AttachmentDTO?> GetAttachmentByIdAsync(Guid id, Guid actingUserId, string? bearerToken);
        Task<string?> GetDownloadUrlAsync(Guid id, Guid actingUserId, string? bearerToken);
        Task<AttachmentDetailsDTO?> GetAttachmentDetailsAsync(Guid id, Guid actingUserId, string? bearerToken);
        Task<AttachmentUploadResponseDTO> CreateAttachmentAsync(AttachmentCreationDTO attachment, Guid uploadedByUserId, string? bearerToken);
        Task<ConfirmAttachmentResult> ConfirmAttachmentAsync(AttachmentConfirmationDTO confirmation, Guid actingUserId, string? bearerToken);
        Task<AttachmentDTO?> UpdateAttachmentAsync(Guid id, AttachmentUpdateDTO attachment, Guid actingUserId, string? bearerToken);
        Task<bool> DeleteAttachmentAsync(Guid id, Guid actingUserId, string? bearerToken);
    }
}
