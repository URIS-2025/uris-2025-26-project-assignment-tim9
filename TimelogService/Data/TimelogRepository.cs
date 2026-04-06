using TimelogService.Models;
using TimelogService.Models.DTO;
using TimelogService.Models.DTO.Project;
using TimelogService.Models.DTO.WorkPackage;
using AutoMapper;

namespace TimelogService.Data
{
    public class TimelogRepository : ITimelogRepository
    {
        private List<Timelog> Timelogs;
        private List<ProjectDTO> Projects;
        private List<WorkPackageDTO> WorkPackages;
        private readonly IMapper _mapper;

        public TimelogRepository(IMapper mapper)
        {
            _mapper = mapper;
            Timelogs = new List<Timelog>();
            Projects = new List<ProjectDTO>();
            WorkPackages = new List<WorkPackageDTO>();

            FillData();
        }

        private void FillData()
        {
            var projectId = Guid.Parse("044f3de0-a9dd-4c2e-b745-89976a1b2a36");
            var wpId = Guid.Parse("21ad52f8-0281-4241-98b0-481566d25e4f");

            Projects.Add(new ProjectDTO
            {
                Username = "milabikar",
                Email = "milabikar@gmail.com"
            });

            WorkPackages.Add(new WorkPackageDTO
            {
                Title = "Repository implementation",
                Status = "Doing"
            });

            Timelogs.Add(new Timelog
            {
                Id = Guid.Parse("7a411c13-a195-48f7-8dbd-67596c3974c0"),
                ProjectId = projectId,
                WorkPackageId = wpId,
                HoursSpent = 4.5,
                Date = DateTime.Now
            });
        }

        public IEnumerable<TimelogDTO> GetTimelogs()
        {
            return _mapper.Map<List<TimelogDTO>>(Timelogs);
        }

        public TimelogDTO GetTimelogById(Guid id)
        {
            var log = Timelogs.FirstOrDefault(t => t.Id == id);
            return _mapper.Map<TimelogDTO>(log)!;
        }

        public TimelogConfirmationDTO CreateTimelog(TimelogCreationDTO timelog)
        {
            var newTimelog = _mapper.Map<Timelog>(timelog);
            newTimelog.Id = Guid.NewGuid();

            Timelogs.Add(newTimelog);

            var confirmation = _mapper.Map<TimelogConfirmationDTO>(newTimelog);

            confirmation.Username = Projects.FirstOrDefault()?.Username ?? "Unknown";
            confirmation.WorkPackageTitle = WorkPackages.FirstOrDefault()?.Title ?? "No Title";

            return confirmation;
        }

        public TimelogConfirmationDTO UpdateTimelog(Timelog timelog)
        {
            var existingLog = Timelogs.FirstOrDefault(t => t.Id == timelog.Id);
            if (existingLog != null)
            {
                _mapper.Map(timelog, existingLog);
            }

            var confirmation = _mapper.Map<TimelogConfirmationDTO>(existingLog);
            confirmation.Username = Projects.FirstOrDefault()?.Username ?? "Unknown";
            confirmation.WorkPackageTitle = WorkPackages.FirstOrDefault()?.Title ?? "No Title";

            return confirmation;
        }

        public void DeleteTimelog(Guid id)
        {
            var log = Timelogs.FirstOrDefault(t => t.Id == id);
            if (log != null)
            {
                Timelogs.Remove(log);
            }
        }
    }
}