using AutoMapper;
using SprintService.Context;
using SprintService.Models;
using SprintService.Models.DTO;
using SprintService.ServiceCalls.Project;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SprintService.Data
{
    public class SprintRepository : ISprintRepository
    {
        private readonly SprintContext _context;
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;

        public SprintRepository(SprintContext context, IMapper mapper, IProjectService projectService)
        {
            _context = context;
            _mapper = mapper;
            _projectService = projectService;
        }

        public IEnumerable<SprintDTO> GetSprints()
        {
            var sprints = _context.Sprints.ToList();
            return _mapper.Map<List<SprintDTO>>(sprints);
        }

        public SprintDTO GetSprintById(Guid id)
        {
            var sprint = _context.Sprints.FirstOrDefault(s => s.Id == id);
            return _mapper.Map<SprintDTO>(sprint);
        }

        public SprintConfirmationDTO CreateSprint(SprintCreationDTO sprint)
        {
            var newSprint = _mapper.Map<Sprint>(sprint);
            newSprint.Id = Guid.NewGuid();

            _context.Sprints.Add(newSprint);
            SaveChanges();

            var confirmation = _mapper.Map<SprintConfirmationDTO>(newSprint);

            var projectData = _projectService.GetProjectById(newSprint.ProjectId);
            if (projectData != null)
            {
                confirmation.MilestoneId = projectData.MilestoneID;
                confirmation.ExpectedDate = projectData.ExpectedDate;
            }

            return confirmation;
        }

        public SprintConfirmationDTO UpdateSprint(Sprint sprint)
        {
            var existingSprint = _context.Sprints.FirstOrDefault(s => s.Id == sprint.Id);
            if (existingSprint != null)
            {
                _mapper.Map(sprint, existingSprint);
                SaveChanges();
            }

            var confirmation = _mapper.Map<SprintConfirmationDTO>(existingSprint);

            var projectData = _projectService.GetProjectById(existingSprint.ProjectId);
            if (projectData != null)
            {
                confirmation.MilestoneId = projectData.MilestoneID;
                confirmation.ExpectedDate = projectData.ExpectedDate;
            }

            return confirmation;

        }

        public void DeleteSprint(Guid id)
        {
            var sprint = _context.Sprints.FirstOrDefault(s => s.Id == id);
            if (sprint != null)
            {
                _context.Sprints.Remove(sprint);
                SaveChanges();
            }
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() >= 0;
        }
    }
}