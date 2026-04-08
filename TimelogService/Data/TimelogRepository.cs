using AutoMapper;
using TimelogService.Context;
using TimelogService.Models;
using TimelogService.Models.DTO;
using TimelogService.Models.DTO.Project;
using TimelogService.Models.DTO.WorkPackage;
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

        public TimelogRepository(IMapper mapper, TimelogContext context, IProjectService projectService, IWorkPackageService wpService)
        {
            _mapper = mapper;
            _context = context;
            _projectService = projectService;
            _wpService = wpService;
        }

        public bool SaveChanges()
        {
            return (_context.SaveChanges() >= 0);
        }

        public IEnumerable<TimelogDTO> GetTimelogs()
        {
            var timelogs = _context.Timelogs.ToList();
            return _mapper.Map<List<TimelogDTO>>(timelogs);
        }

        public TimelogDTO GetTimelogById(Guid id)
        {
            var log = _context.Timelogs.FirstOrDefault(t => t.Id == id);
            return _mapper.Map<TimelogDTO>(log)!;
        }

        public TimelogConfirmationDTO CreateTimelog(TimelogCreationDTO timelog)
        {
            var newTimelog = _mapper.Map<Timelog>(timelog);
            newTimelog.Id = Guid.NewGuid();

            _context.Timelogs.Add(newTimelog);
            SaveChanges();

            var confirmation = _mapper.Map<TimelogConfirmationDTO>(newTimelog);
          
            var projectData = _projectService.GetProjectById(newTimelog.ProjectId);
            var wpData = _wpService.GetWorkPackageById(newTimelog.WorkPackageId);

            confirmation.Username = projectData?.Username ?? "Unknown User";
            confirmation.WorkPackageTitle = wpData?.Title ?? "Unknown WorkPackage";

            return confirmation;
        }

        public TimelogConfirmationDTO UpdateTimelog(Timelog timelog)
        {
            var existingLog = _context.Timelogs.FirstOrDefault(t => t.Id == timelog.Id);
            if (existingLog != null)
            {
                _mapper.Map(timelog, existingLog);
                SaveChanges();
            }

            var confirmation = _mapper.Map<TimelogConfirmationDTO>(existingLog);

            var projectData = _projectService.GetProjectById(existingLog.ProjectId);
            confirmation.Username = projectData?.Username ?? "Unknown User";

            return confirmation;
        }

        public void DeleteTimelog(Guid id)
        {
            var log = _context.Timelogs.FirstOrDefault(t => t.Id == id);
            if (log != null)
            {
                _context.Timelogs.Remove(log);
                SaveChanges();
            }
        }
    }
}