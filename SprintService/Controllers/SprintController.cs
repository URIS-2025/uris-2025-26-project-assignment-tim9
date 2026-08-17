using Microsoft.AspNetCore.Mvc;
using SprintService.Data;
using SprintService.Models.DTO;

namespace SprintService.Controllers
{
    [ApiController]
    [Route("sprints")]
    public class SprintController : ControllerBase
    {
        private readonly ISprintRepository _sprintRepository;

        public SprintController(ISprintRepository sprintRepository)
        {
            _sprintRepository = sprintRepository;
        }

        // GET: /sprints
        // GET: /sprints?projectId={id} - extra convenience, same filter as the required
        // GET /projects/{projectId}/sprints route below.
        [HttpGet]
        public ActionResult<IEnumerable<SprintDTO>> GetSprints([FromQuery] Guid? projectId)
        {
            return Ok(_sprintRepository.GetSprints(projectId));
        }

        // GET: /projects/{projectId}/sprints
        [HttpGet("~/projects/{projectId:guid}/sprints")]
        public ActionResult<IEnumerable<SprintDTO>> GetSprintsForProject(Guid projectId)
        {
            return Ok(_sprintRepository.GetSprints(projectId));
        }

        // GET: /sprints/{sprintId}
        [HttpGet("{sprintId:guid}", Name = "GetSprintById")]
        public ActionResult<SprintDTO> GetSprintById(Guid sprintId)
        {
            var sprint = _sprintRepository.GetSprintById(sprintId);
            if (sprint is null)
            {
                return NotFound();
            }
            return Ok(sprint);
        }

        // POST: /projects/{projectId}/sprints
        [HttpPost("~/projects/{projectId:guid}/sprints")]
        public async Task<ActionResult<SprintConfirmationDTO>> CreateSprint(Guid projectId, SprintCreationDTO sprint)
        {
            var confirmation = await _sprintRepository.CreateSprintAsync(projectId, sprint);
            return CreatedAtRoute("GetSprintById", new { sprintId = confirmation.Id }, confirmation);
        }

        // DELETE: /sprints/{sprintId}
        [HttpDelete("{sprintId:guid}")]
        public IActionResult DeleteSprint(Guid sprintId)
        {
            if (_sprintRepository.GetSprintById(sprintId) is null)
            {
                return NotFound();
            }

            _sprintRepository.DeleteSprint(sprintId);
            return NoContent();
        }

        // PUT: /sprints/{sprintId}
        [HttpPut("{sprintId:guid}")]
        public async Task<ActionResult<SprintConfirmationDTO>> UpdateSprint(Guid sprintId, SprintUpdateDTO sprint)
        {
            var updated = await _sprintRepository.UpdateSprintAsync(sprintId, sprint);
            if (updated is null)
            {
                return NotFound();
            }
            return Ok(updated);
        }
    }
}
