using AutoMapper;
using AttachmentService.Context;
using AttachmentService.Models;
using AttachmentService.Models.DTO;
using AttachmentService.Models.Enums;
using AttachmentService.ServiceCalls.Project;
using AttachmentService.ServiceCalls.WorkPackage;
using AttachmentService.Storage;

namespace AttachmentService.Data
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly AttachmentContext _context;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;
        private readonly IWorkPackageService _workPackageService;
        private readonly IProjectService _projectService;

        public AttachmentRepository(
            AttachmentContext context,
            IMapper mapper,
            IFileStorageService fileStorageService,
            IWorkPackageService workPackageService,
            IProjectService projectService)
        {
            _context = context;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
            _workPackageService = workPackageService;
            _projectService = projectService;
        }

        public IEnumerable<AttachmentDTO> GetAttachments(Guid? projectId = null, Guid? workPackageId = null)
        {
            var query = _context.Attachments.Where(a => a.Status != AttachmentStatus.Deleted);

            if (workPackageId.HasValue)
            {
                query = query.Where(a => a.WorkPackageId == workPackageId.Value);
            }
            else if (projectId.HasValue)
            {
                query = query.Where(a => a.ProjectId == projectId.Value);
            }

            var attachments = query
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

        public string? GetDownloadUrl(Guid id)
        {
            var attachment = _context.Attachments
                .FirstOrDefault(a => a.Id == id && a.Status == AttachmentStatus.Ready);

            return attachment is null
                ? null
                : _fileStorageService.GenerateDownloadUrl(attachment.StoragePath);
        }

        public async Task<AttachmentDetailsDTO?> GetAttachmentDetailsAsync(Guid id)
        {
            var attachment = GetAttachmentById(id);

            if (attachment is null)
            {
                return null;
            }

            string? workPackageTitle = null;
            if (attachment.WorkPackageId is Guid workPackageId)
            {
                var workPackage = await _workPackageService.GetWorkPackageByIdAsync(workPackageId);
                workPackageTitle = workPackage?.Title;
            }

            var uploader = await _projectService.GetUserInfoAsync(attachment.UploadedByUserId);

            return new AttachmentDetailsDTO
            {
                Attachment = attachment,
                WorkPackageTitle = workPackageTitle,
                UploadedByUsername = uploader?.Username,
                UploadedByRole = uploader?.Role
            };
        }

        public AttachmentUploadResponseDTO CreateAttachment(AttachmentCreationDTO attachment, Guid uploadedByUserId)
        {
            var entity = _mapper.Map<Attachment>(attachment);

            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.Status = AttachmentStatus.Uploading;
            entity.FileName = $"{entity.Id}_{attachment.OriginalFileName}";
            entity.StoragePath = BuildStoragePath(entity);
            entity.UploadedByUserId = uploadedByUserId;

            _context.Attachments.Add(entity);
            _context.SaveChanges();

            var uploadUrl = _fileStorageService.GenerateUploadUrl(entity.StoragePath, entity.ContentType);

            return new AttachmentUploadResponseDTO
            {
                Attachment = _mapper.Map<AttachmentDTO>(entity),
                UploadUrl = uploadUrl
            };
        }

        public async Task<ConfirmAttachmentResult> ConfirmAttachmentAsync(AttachmentConfirmationDTO confirmation)
        {
            var entity = _context.Attachments.FirstOrDefault(a => a.Id == confirmation.AttachmentId);

            if (entity is null || entity.Status == AttachmentStatus.Deleted)
            {
                return new ConfirmAttachmentResult(ConfirmAttachmentOutcome.NotFound, null);
            }

            if (entity.Status != AttachmentStatus.Uploading)
            {
                return new ConfirmAttachmentResult(ConfirmAttachmentOutcome.InvalidState, null);
            }

            if (!await _fileStorageService.ObjectExistsAsync(entity.StoragePath))
            {
                return new ConfirmAttachmentResult(ConfirmAttachmentOutcome.ObjectMissing, null);
            }

            if (confirmation.Checksum is not null)
            {
                entity.Checksum = confirmation.Checksum;
            }

            entity.Status = AttachmentStatus.Ready;
            _context.SaveChanges();

            return new ConfirmAttachmentResult(ConfirmAttachmentOutcome.Success, _mapper.Map<AttachmentDTO>(entity));
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
