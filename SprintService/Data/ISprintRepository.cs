using System;
using System.Collections.Generic;
using SprintService.Models;
using SprintService.Models.DTO;

namespace SprintService.Data
{
    public interface ISprintRepository
    {
        IEnumerable<SprintDTO> GetSprints();
        SprintDTO GetSprintById(Guid id);
        SprintConfirmationDTO CreateSprint(SprintCreationDTO sprint);
        SprintConfirmationDTO UpdateSprint(Sprint sprint);
        void DeleteSprint(Guid id);
        bool SaveChanges();
    }
}