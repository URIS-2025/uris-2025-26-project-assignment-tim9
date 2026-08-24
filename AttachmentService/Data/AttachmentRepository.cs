using AutoMapper;
using AttachmentService.Context;
using AttachmentService.Exceptions;
using AttachmentService.Models;
using AttachmentService.Models.DTO;
using AttachmentService.Models.DTO.User;
using AttachmentService.Models.Enums;
using AttachmentService.ServiceCalls.Project;
using AttachmentService.ServiceCalls.User;
using AttachmentService.ServiceCalls.WorkPackage;
using AttachmentService.Storage;

namespace AttachmentService.Data
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private const string AdminRole = "Admin";
        private static readonly string[] UploaderRoles = { "TeamMember", "ProjectManager" };

        private readonly AttachmentContext _context;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;
        private readonly ITaskService _taskService;
        private readonly IProjectService _projectService;
        private readonly IUserService _userService;

        public AttachmentRepository(
            AttachmentContext context,
            IMapper mapper,
            IFileStorageService fileStorageService,
            ITaskService taskService,
            IProjectService projectService,
            IUserService userService)
        {
            _context = context;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
            _taskService = taskService;
            _projectService = projectService;
            _userService = userService;
        }

        public async Task<IEnumerable<AttachmentDTO>> GetAttachmentsAsync(Guid? projectId, Guid? taskId, Guid actingUserId, string? bearerToken)
        {
            if (projectId is null && taskId is null)
            {
                var userInfo = await _userService.GetUserInfoAsync(actingUserId);
                if (!IsConfirmedAdmin(userInfo))
                {
                    throw new ProjectContextRequiredException(actingUserId);
                }

                return _mapper.Map<List<AttachmentDTO>>(QueryAttachments(null, null));
            }

            var candidates = QueryAttachments(projectId, taskId);
            if (candidates.Count == 0)
            {
                return Enumerable.Empty<AttachmentDTO>();
            }

            var effectiveProjectId = projectId ?? candidates[0].ProjectId;
            await EnsureCanViewProjectAsync(actingUserId, effectiveProjectId, bearerToken);

            return _mapper.Map<List<AttachmentDTO>>(candidates);
        }

        public async Task<AttachmentDTO?> GetAttachmentByIdAsync(Guid id, Guid actingUserId, string? bearerToken)
        {
            var attachment = _context.Attachments
                .FirstOrDefault(a => a.Id == id && a.Status != AttachmentStatus.Deleted);

            if (attachment is null)
            {
                return null;
            }

            await EnsureCanViewProjectAsync(actingUserId, attachment.ProjectId, bearerToken);

            return _mapper.Map<AttachmentDTO>(attachment);
        }

        public async Task<string?> GetDownloadUrlAsync(Guid id, Guid actingUserId, string? bearerToken)
        {
            var attachment = _context.Attachments
                .FirstOrDefault(a => a.Id == id && a.Status == AttachmentStatus.Ready);

            if (attachment is null)
            {
                return null;
            }

            await EnsureCanViewProjectAsync(actingUserId, attachment.ProjectId, bearerToken);

            return _fileStorageService.GenerateDownloadUrl(attachment.StoragePath);
        }

        public async Task<AttachmentDetailsDTO?> GetAttachmentDetailsAsync(Guid id, Guid actingUserId, string? bearerToken)
        {
            var attachment = _context.Attachments
                .FirstOrDefault(a => a.Id == id && a.Status != AttachmentStatus.Deleted);

            if (attachment is null)
            {
                return null;
            }

            await EnsureCanViewProjectAsync(actingUserId, attachment.ProjectId, bearerToken);

            var attachmentDto = _mapper.Map<AttachmentDTO>(attachment);

            string? taskTitle = null;
            if (attachment.TaskId is Guid taskId)
            {
                var taskLookup = await _taskService.GetTaskByIdAsync(taskId, bearerToken);
                taskTitle = taskLookup.Task?.Title;
            }

            var uploader = await _userService.GetUserInfoAsync(attachment.UploadedByUserId);

            return new AttachmentDetailsDTO
            {
                Attachment = attachmentDto,
                TaskTitle = taskTitle,
                UploadedByUsername = uploader?.Username,
                UploadedByRole = uploader?.Role
            };
        }

        public async Task<AttachmentUploadResponseDTO> CreateAttachmentAsync(AttachmentCreationDTO attachment, Guid uploadedByUserId, string? bearerToken)
        {
            await EnsureProjectExistsAsync(attachment.ProjectId, bearerToken);
            if (attachment.TaskId is Guid taskId)
            {
                await EnsureTaskExistsAsync(taskId, bearerToken);
            }

            var userInfo = await _userService.GetUserInfoAsync(uploadedByUserId);
            await EnsureAllowedToUploadAsync(userInfo, uploadedByUserId, attachment.ProjectId, bearerToken);

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
                UploadUrl = uploadUrl,
                UploadedByUsername = userInfo?.Username ?? "Unknown User",
                UploadedByRole = userInfo?.Role ?? "Unknown"
            };
        }

        public async Task<ConfirmAttachmentResult> ConfirmAttachmentAsync(AttachmentConfirmationDTO confirmation, Guid actingUserId, string? bearerToken)
        {
            var entity = _context.Attachments.FirstOrDefault(a => a.Id == confirmation.AttachmentId);

            if (entity is null || entity.Status == AttachmentStatus.Deleted)
            {
                return new ConfirmAttachmentResult(ConfirmAttachmentOutcome.NotFound, null);
            }

            if (!await IsOwnerOrAdminAsync(actingUserId, entity.UploadedByUserId, bearerToken))
            {
                return new ConfirmAttachmentResult(ConfirmAttachmentOutcome.Forbidden, null);
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

        public async Task<AttachmentDTO?> UpdateAttachmentAsync(Guid id, AttachmentUpdateDTO attachment, Guid actingUserId, string? bearerToken)
        {
            var entity = _context.Attachments
                .FirstOrDefault(a => a.Id == id && a.Status != AttachmentStatus.Deleted);

            if (entity is null)
            {
                return null;
            }

            await EnsureOwnerOrAdminAsync(actingUserId, entity.UploadedByUserId, bearerToken);

            _mapper.Map(attachment, entity);
            _context.SaveChanges();

            return _mapper.Map<AttachmentDTO>(entity);
        }

        public async Task<bool> DeleteAttachmentAsync(Guid id, Guid actingUserId, string? bearerToken)
        {
            var entity = _context.Attachments.FirstOrDefault(a => a.Id == id);

            if (entity is null)
            {
                return false;
            }

            if (entity.Status == AttachmentStatus.Deleted)
            {
                return true;
            }

            await EnsureOwnerOrAdminAsync(actingUserId, entity.UploadedByUserId, bearerToken);

            entity.Status = AttachmentStatus.Deleted;
            entity.DeletedAt = DateTime.UtcNow;
            _context.SaveChanges();
            return true;
        }

        private List<Attachment> QueryAttachments(Guid? projectId, Guid? taskId)
        {
            var query = _context.Attachments.Where(a => a.Status != AttachmentStatus.Deleted);

            if (taskId.HasValue)
            {
                query = query.Where(a => a.TaskId == taskId.Value);
            }
            else if (projectId.HasValue)
            {
                query = query.Where(a => a.ProjectId == projectId.Value);
            }

            return query.OrderByDescending(a => a.CreatedAt).ToList();
        }

        private async Task EnsureProjectExistsAsync(Guid projectId, string? bearerToken)
        {
            var result = await _projectService.CheckProjectExistsAsync(projectId, bearerToken);
            if (result.Status == ProjectExistsStatus.NotFound)
            {
                throw new ProjectNotFoundException(projectId);
            }
        }

        private async Task EnsureTaskExistsAsync(Guid taskId, string? bearerToken)
        {
            var result = await _taskService.GetTaskByIdAsync(taskId, bearerToken);
            if (result.Status == TaskLookupStatus.NotFound)
            {
                throw new TaskNotFoundException(taskId);
            }
        }

        private async Task EnsureCanViewProjectAsync(Guid userId, Guid projectId, string? bearerToken)
        {
            var userInfo = await _userService.GetUserInfoAsync(userId);
            if (IsConfirmedAdmin(userInfo))
            {
                return;
            }

            var membership = await _projectService.CheckMembershipAsync(projectId, userId, bearerToken);
            if (membership.Status == ProjectMembershipStatus.NotMember)
            {
                throw new UserNotProjectMemberException(userId, projectId);
            }
        }

        private async Task EnsureAllowedToUploadAsync(UserInfoDTO? userInfo, Guid userId, Guid projectId, string? bearerToken)
        {
            if (IsConfirmedAdmin(userInfo))
            {
                return;
            }

            if (userInfo is not null && !UploaderRoles.Contains(userInfo.Role, StringComparer.OrdinalIgnoreCase))
            {
                throw new RoleCannotUploadAttachmentsException(userId, userInfo.Role);
            }

            var membership = await _projectService.CheckMembershipAsync(projectId, userId, bearerToken);
            if (membership.Status == ProjectMembershipStatus.NotMember)
            {
                throw new UserNotProjectMemberException(userId, projectId);
            }
        }

        private async Task EnsureOwnerOrAdminAsync(Guid actingUserId, Guid ownerUserId, string? bearerToken)
        {
            if (!await IsOwnerOrAdminAsync(actingUserId, ownerUserId, bearerToken))
            {
                throw new NotAttachmentOwnerException(actingUserId, ownerUserId);
            }
        }

        private async Task<bool> IsOwnerOrAdminAsync(Guid actingUserId, Guid ownerUserId, string? bearerToken)
        {
            if (actingUserId == ownerUserId)
            {
                return true;
            }

            var actingUserInfo = await _userService.GetUserInfoAsync(actingUserId);
            return IsConfirmedAdmin(actingUserInfo);
        }

        private static bool IsConfirmedAdmin(UserInfoDTO? userInfo)
        {
            return userInfo is not null && string.Equals(userInfo.Role, AdminRole, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildStoragePath(Attachment entity)
        {
            return entity.TaskId is Guid taskId
                ? $"projects/{entity.ProjectId}/tasks/{taskId}/{entity.FileName}"
                : $"projects/{entity.ProjectId}/{entity.FileName}";
        }
    }
}
