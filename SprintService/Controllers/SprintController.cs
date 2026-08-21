using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SprintService.Data;
using SprintService.Models.DTO;

namespace SprintService.Controllers
{
    [Authorize]
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
        public async Task<ActionResult<IEnumerable<SprintDTO>>> GetSprints([FromQuery] Guid? projectId)
        {
            try
            {
                var sprints = await _sprintRepository.GetSprintsForCallerAsync(projectId, GetClientUserId());
                return Ok(sprints);
            }
            catch (ProjectNotFoundException)
            {
                return NotFound();
            }
            catch (SprintValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: /projects/{projectId}/sprints
        [HttpGet("~/projects/{projectId:guid}/sprints")]
        public async Task<ActionResult<IEnumerable<SprintDTO>>> GetSprintsForProject(Guid projectId)
        {
            try
            {
                var sprints = await _sprintRepository.GetSprintsForCallerAsync(projectId, GetClientUserId());
                return Ok(sprints);
            }
            catch (ProjectNotFoundException)
            {
                return NotFound();
            }
            catch (SprintValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: /sprints/{sprintId}
        [HttpGet("{sprintId:guid}", Name = "GetSprintById")]
        public async Task<ActionResult<SprintDTO>> GetSprintById(Guid sprintId)
        {
            var sprint = await _sprintRepository.GetSprintByIdForCallerAsync(sprintId, GetClientUserId());
            if (sprint is null)
            {
                return NotFound();
            }
            return Ok(sprint);
        }

        // POST: /projects/{projectId}/sprints
        [HttpPost("~/projects/{projectId:guid}/sprints")]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<ActionResult<SprintConfirmationDTO>> CreateSprint(Guid projectId, SprintCreationDTO sprint)
        {
            try
            {
                var confirmation = await _sprintRepository.CreateSprintAsync(projectId, sprint);
                return CreatedAtRoute("GetSprintById", new { sprintId = confirmation.Id }, confirmation);
            }
            catch (SprintValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: /sprints/{sprintId}
        [HttpDelete("{sprintId:guid}")]
        [Authorize(Roles = "Admin,ProjectManager")]
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
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<ActionResult<SprintConfirmationDTO>> UpdateSprint(Guid sprintId, SprintUpdateDTO sprint)
        {
            try
            {
                var updated = await _sprintRepository.UpdateSprintAsync(sprintId, sprint);
                if (updated is null)
                {
                    return NotFound();
                }
                return Ok(updated);
            }
            catch (SprintValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private Guid? GetClientUserId()
        {
            if (!User.IsInRole("Client"))
            {
                return null;
            }

            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(sub, out var userId) ? userId : Guid.Empty;
        }
    }
}
