using AutoMapper;
using TimelogService.Context;
using TimelogService.Models;
using TimelogService.Models.DTO;
using TimelogService.ServiceCalls.Project;
using TimelogService.ServiceCalls.WorkPackage;

namespace TimelogService.Data
{
    public class TimelogRepository : ITimelogRepository
    {
        private readonly TimelogContext _context;
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly IWorkPackageService _wpService;

        public TimelogRepository(TimelogContext context, IMapper mapper, IProjectService projectService, IWorkPackageService wpService)
        {
            _context = context;
            _mapper = mapper;
            _projectService = projectService;
            _wpService = wpService;
        }

        public IEnumerable<TimelogDTO> GetTimelogs(Guid? projectId = null, Guid? workPackageId = null)
        {
            var query = _context.Timelogs.AsQueryable();

            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId.Value);
            }

            if (workPackageId.HasValue)
            {
                query = query.Where(t => t.WorkPackageId == workPackageId.Value);
            }

            var timelogs = query.ToList();
            return _mapper.Map<List<TimelogDTO>>(timelogs);
        }

        public TimelogDTO? GetTimelogById(Guid id)
        {
            var log = _context.Timelogs.FirstOrDefault(t => t.Id == id);
            return log is null ? null : _mapper.Map<TimelogDTO>(log);
        }

        public async Task<TimelogConfirmationDTO> CreateTimelogAsync(TimelogCreationDTO timelog, Guid loggedByUserId)
        {
            var newTimelog = _mapper.Map<Timelog>(timelog);
            newTimelog.Id = Guid.NewGuid();
            newTimelog.LoggedByUserId = loggedByUserId;

            _context.Timelogs.Add(newTimelog);
            _context.SaveChanges();

            var confirmation = _mapper.Map<TimelogConfirmationDTO>(newTimelog);

            var userInfo = await _projectService.GetUserInfoAsync(newTimelog.LoggedByUserId);
            var wpData = await _wpService.GetWorkPackageByIdAsync(newTimelog.WorkPackageId);

            confirmation.Username = userInfo?.Username ?? "Unknown User";
            confirmation.WorkPackageTitle = wpData?.Title ?? "Unknown WorkPackage";

            return confirmation;
        }

        public async Task<TimelogConfirmationDTO?> UpdateTimelogAsync(Guid id, TimelogUpdateDTO timelog)
        {
            var existingLog = _context.Timelogs.FirstOrDefault(t => t.Id == id);
            if (existingLog is null)
            {
                return null;
            }

            _mapper.Map(timelog, existingLog);
            _context.SaveChanges();

            var confirmation = _mapper.Map<TimelogConfirmationDTO>(existingLog);

            var userInfo = await _projectService.GetUserInfoAsync(existingLog.LoggedByUserId);
            var wpData = await _wpService.GetWorkPackageByIdAsync(existingLog.WorkPackageId);

            confirmation.Username = userInfo?.Username ?? "Unknown User";
            confirmation.WorkPackageTitle = wpData?.Title ?? "Unknown WorkPackage";

            return confirmation;
        }

        public void DeleteTimelog(Guid id)
        {
            var log = _context.Timelogs.FirstOrDefault(t => t.Id == id);
            if (log is not null)
            {
                _context.Timelogs.Remove(log);
                _context.SaveChanges();
            }
        }
    }
}
