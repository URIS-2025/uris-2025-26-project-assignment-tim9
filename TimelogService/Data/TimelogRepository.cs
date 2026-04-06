using TimelogService.Models;
using TimelogService.Models.DTO;
using TimelogService.Models.DTO.Project;
using TimelogService.Models.DTO.WorkPackage;

namespace TimelogService.Data
{
    public class TimelogRepository : ITimelogRepository
    {
        private List<Timelog> Timelogs;
        private List<ProjectDTO> Projects;
        private List<WorkPackageDTO> WorkPackages;

        public TimelogRepository()
        {
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
                Title = "Repository implemantion",
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
            var timelogDTOs = new List<TimelogDTO>();
            foreach (var log in Timelogs)
            {
                timelogDTOs.Add(new TimelogDTO
                {
                    Id = log.Id,
                    ProjectId = log.ProjectId,
                    WorkPackageId = log.WorkPackageId,
                    HoursSpent = log.HoursSpent,
                    Date = log.Date
                });
            }
            return timelogDTOs;
        }

        public TimelogDTO GetTimelogById(Guid id)
        {
            var log = Timelogs.FirstOrDefault(t => t.Id == id);
            if (log == null) return null!;

            return new TimelogDTO
            {
                Id = log.Id,
                ProjectId = log.ProjectId,
                WorkPackageId = log.WorkPackageId,
                HoursSpent = log.HoursSpent,
                Date = log.Date
            };
        }

        public TimelogConfirmationDTO CreateTimelog(TimelogCreationDTO timelog)
        {
            var newTimelog = new Timelog
            {
                Id = Guid.NewGuid(),
                ProjectId = timelog.ProjectId,
                WorkPackageId = timelog.WorkPackageId,
                HoursSpent = timelog.HoursSpent,
                Date = timelog.Date
            };

            Timelogs.Add(newTimelog);

            return new TimelogConfirmationDTO
            {
                Id = newTimelog.Id,
                Username = Projects.FirstOrDefault()?.Username ?? "Unknown",
                WorkPackageTitle = WorkPackages.FirstOrDefault()?.Title ?? "No Title",
                HoursSpent = newTimelog.HoursSpent,
                Date = newTimelog.Date
            };
        }

        public TimelogConfirmationDTO UpdateTimelog(Timelog timelog)
        {
            var existingLog = Timelogs.FirstOrDefault(t => t.Id == timelog.Id);
            if (existingLog != null)
            {
                existingLog.ProjectId = timelog.ProjectId;
                existingLog.WorkPackageId = timelog.WorkPackageId;
                existingLog.HoursSpent = timelog.HoursSpent;
                existingLog.Date = timelog.Date;
            }

            return new TimelogConfirmationDTO
            {
                Id = existingLog?.Id ?? Guid.Empty,
                Username = Projects.FirstOrDefault()?.Username ?? "Unknown",
                WorkPackageTitle = WorkPackages.FirstOrDefault()?.Title ?? "No Title",
                HoursSpent = existingLog?.HoursSpent ?? 0,
                Date = existingLog?.Date ?? DateTime.Now
            };
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