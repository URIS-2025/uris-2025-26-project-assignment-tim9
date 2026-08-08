using AutoMapper;
using AttachmentService.Context;
using AttachmentService.Models;
using AttachmentService.Models.DTO;
using AttachmentService.Models.Enums;

namespace AttachmentService.Data
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly AttachmentContext _context;
        private readonly IMapper _mapper;

        public AttachmentRepository(AttachmentContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<AttachmentDTO> GetAttachments()
        {
            var attachments = _context.Attachments
                .Where(a => a.Status != AttachmentStatus.Deleted)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            return _mapper.Map<List<AttachmentDTO>>(attachments);
        }

        public AttachmentDTO? GetAttachmentById(Guid id)
        {
            var attachment = _context.Attachments
                .FirstOrDefault(a => a.Id == id && a.Status != AttachmentStatus.Deleted);

            return attachment is null ? null : _mapper.Map<AttachmentDTO>(attachment);
        }

        public AttachmentDTO CreateAttachment(AttachmentCreationDTO attachment)
        {
            var entity = _mapper.Map<Attachment>(attachment);

            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.Status = AttachmentStatus.Uploading;
            entity.FileName = $"{entity.Id}_{attachment.OriginalFileName}";
            entity.StoragePath = BuildStoragePath(entity);

            _context.Attachments.Add(entity);
            _context.SaveChanges();

            return _mapper.Map<AttachmentDTO>(entity);
        }

        public AttachmentDTO? ConfirmAttachment(AttachmentConfirmationDTO confirmation)
        {
            var entity = _context.Attachments.FirstOrDefault(a => a.Id == confirmation.AttachmentId);

            if (entity is null || entity.Status != AttachmentStatus.Uploading)
            {
                return null;
            }

            if (confirmation.Checksum is not null)
            {
                entity.Checksum = confirmation.Checksum;
            }

            entity.Status = AttachmentStatus.Ready;
            _context.SaveChanges();

            return _mapper.Map<AttachmentDTO>(entity);
        }

        public AttachmentDTO? UpdateAttachment(Guid id, AttachmentUpdateDTO attachment)
        {
            var entity = _context.Attachments
                .FirstOrDefault(a => a.Id == id && a.Status != AttachmentStatus.Deleted);

            if (entity is null)
            {
                return null;
            }

            _mapper.Map(attachment, entity);
            _context.SaveChanges();

            return _mapper.Map<AttachmentDTO>(entity);
        }

        public void DeleteAttachment(Guid id)
        {
            var entity = _context.Attachments.FirstOrDefault(a => a.Id == id);

            if (entity is null || entity.Status == AttachmentStatus.Deleted)
            {
                return;
            }

            entity.Status = AttachmentStatus.Deleted;
            entity.DeletedAt = DateTime.UtcNow;
            _context.SaveChanges();
        }

        private static string BuildStoragePath(Attachment entity)
        {
            return entity.WorkPackageId is Guid workPackageId
                ? $"projects/{entity.ProjectId}/workpackages/{workPackageId}/{entity.FileName}"
                : $"projects/{entity.ProjectId}/{entity.FileName}";
        }
    }
}
