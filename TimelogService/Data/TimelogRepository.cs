using AutoMapper;
using TimelogService.Context;
using TimelogService.Exceptions;
using TimelogService.Models;
using TimelogService.Models.DTO;
using TimelogService.Models.DTO.User;
using TimelogService.ServiceCalls.Project;
using TimelogService.ServiceCalls.User;
using TimelogService.ServiceCalls.WorkPackage;

namespace TimelogService.Data
{
    public class TimelogRepository : ITimelogRepository
    {
        private const string ClientRole = "Client";
        private const string AdminRole = "Admin";

        private readonly TimelogContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ITaskService _taskService;
        private readonly IProjectService _projectService;

        public TimelogRepository(TimelogContext context, IMapper mapper, IUserService userService, ITaskService taskService, IProjectService projectService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
            _taskService = taskService;
            _projectService = projectService;
        }

        public IEnumerable<TimelogDTO> GetTimelogs(Guid? projectId = null, Guid? taskId = null)
        {
            var query = _context.Timelogs.AsQueryable();

            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId.Value);
            }

            if (taskId.HasValue)
            {
                query = query.Where(t => t.TaskId == taskId.Value);
            }

            var timelogs = query.ToList();
            return _mapper.Map<List<TimelogDTO>>(timelogs);
        }

        public TimelogDTO? GetTimelogById(Guid id)
        {
            var log = _context.Timelogs.FirstOrDefault(t => t.Id == id);
            return log is null ? null : _mapper.Map<TimelogDTO>(log);
        }

        public async Task<TimelogConfirmationDTO> CreateTimelogAsync(TimelogCreationDTO timelog, Guid loggedByUserId, string? bearerToken)
        {
            await EnsureProjectExistsAsync(timelog.ProjectId, bearerToken);

            var userInfo = await _userService.GetUserInfoAsync(loggedByUserId);
            await EnsureAllowedToLogTimeAsync(userInfo, loggedByUserId, timelog.ProjectId, bearerToken);

            var taskLookup = await _taskService.GetTaskByIdAsync(timelog.TaskId, bearerToken);
            if (taskLookup.Status == TaskLookupStatus.NotFound)
            {
                throw new TaskNotFoundException(timelog.TaskId);
            }

            var newTimelog = _mapper.Map<Timelog>(timelog);
            newTimelog.Id = Guid.NewGuid();
            newTimelog.LoggedByUserId = loggedByUserId;

            _context.Timelogs.Add(newTimelog);
            _context.SaveChanges();

            return EnrichConfirmation(_mapper.Map<TimelogConfirmationDTO>(newTimelog), userInfo, taskLookup);
        }

        public async Task<TimelogConfirmationDTO?> UpdateTimelogAsync(Guid id, TimelogUpdateDTO timelog, Guid actingUserId, string? bearerToken)
        {
            var existingLog = _context.Timelogs.FirstOrDefault(t => t.Id == id);
            if (existingLog is null)
            {
                return null;
            }

            await EnsureOwnerOrAdminAsync(actingUserId, existingLog.LoggedByUserId, bearerToken);

            _mapper.Map(timelog, existingLog);

            await EnsureProjectExistsAsync(existingLog.ProjectId, bearerToken);

            var userInfo = await _userService.GetUserInfoAsync(existingLog.LoggedByUserId);
            await EnsureAllowedToLogTimeAsync(userInfo, existingLog.LoggedByUserId, existingLog.ProjectId, bearerToken);

            var taskLookup = await _taskService.GetTaskByIdAsync(existingLog.TaskId, bearerToken);
            if (taskLookup.Status == TaskLookupStatus.NotFound)
            {
                throw new TaskNotFoundException(existingLog.TaskId);
            }

            _context.SaveChanges();

            return EnrichConfirmation(_mapper.Map<TimelogConfirmationDTO>(existingLog), userInfo, taskLookup);
        }

        private async Task EnsureProjectExistsAsync(Guid projectId, string? bearerToken)
        {
            var projectExists = await _projectService.CheckProjectExistsAsync(projectId, bearerToken);
            if (projectExists.Status == ProjectExistsStatus.NotFound)
            {
                throw new ProjectNotFoundException(projectId);
            }
        }

        private async Task EnsureAllowedToLogTimeAsync(UserInfoDTO? userInfo, Guid userId, Guid projectId, string? bearerToken)
        {
            if (userInfo is not null && string.Equals(userInfo.Role, ClientRole, StringComparison.OrdinalIgnoreCase))
            {
                throw new ClientCannotLogTimeException(userId);
            }

            var isAdmin = userInfo is not null && string.Equals(userInfo.Role, AdminRole, StringComparison.OrdinalIgnoreCase);
            if (isAdmin)
            {
                return;
            }

            var membership = await _projectService.CheckMembershipAsync(projectId, userId, bearerToken);
            if (membership.Status == ProjectMembershipStatus.NotMember)
            {
                throw new UserNotProjectMemberException(userId, projectId);
            }
        }

        private async Task EnsureOwnerOrAdminAsync(Guid actingUserId, Guid loggedByUserId, string? bearerToken)
        {
            if (actingUserId == loggedByUserId)
            {
                return;
            }

            var actingUserInfo = await _userService.GetUserInfoAsync(actingUserId);
            var isAdmin = actingUserInfo is not null && string.Equals(actingUserInfo.Role, AdminRole, StringComparison.OrdinalIgnoreCase);
            if (!isAdmin)
            {
                throw new NotTimelogOwnerException(actingUserId, loggedByUserId);
            }
        }

        private static TimelogConfirmationDTO EnrichConfirmation(TimelogConfirmationDTO confirmation, UserInfoDTO? userInfo, TaskLookupResult taskLookup)
        {
            confirmation.Username = userInfo?.Username ?? "Unknown User";
            confirmation.Email = userInfo?.Email ?? "Unknown";
            confirmation.UserRole = userInfo?.Role ?? "Unknown";
            confirmation.TaskTitle = taskLookup.Task?.Title ?? "Unknown Task";
            confirmation.TaskStatus = taskLookup.Task?.Status ?? "Unknown";
            return confirmation;
        }

        public async Task DeleteTimelogAsync(Guid id, Guid actingUserId, string? bearerToken)
        {
            var log = _context.Timelogs.FirstOrDefault(t => t.Id == id);
            if (log is null)
            {
                return;
            }

            await EnsureOwnerOrAdminAsync(actingUserId, log.LoggedByUserId, bearerToken);

            _context.Timelogs.Remove(log);
            _context.SaveChanges();
        }
    }
}
