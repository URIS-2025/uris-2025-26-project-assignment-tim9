using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using SprintService.Context;
using SprintService.Models;
using SprintService.Models.DTO;

namespace SprintService.Data
{
    public class SprintRepository : ISprintRepository
    {
        private readonly SprintContext _context;
        private readonly IMapper _mapper;

        public SprintRepository(SprintContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

            return _mapper.Map<SprintConfirmationDTO>(newSprint);
        }

        public SprintConfirmationDTO UpdateSprint(Sprint sprint)
        {
            var existingSprint = _context.Sprints.FirstOrDefault(s => s.Id == sprint.Id);
            if (existingSprint != null)
            {
                _mapper.Map(sprint, existingSprint);
                SaveChanges();
            }

            return _mapper.Map<SprintConfirmationDTO>(existingSprint);
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