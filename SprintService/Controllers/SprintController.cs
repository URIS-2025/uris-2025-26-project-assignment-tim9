using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SprintService.Data;
using SprintService.Models;
using SprintService.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SprintService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SprintController : ControllerBase
    {
        private readonly ISprintRepository _sprintRepository;

        // Dependency injection
        public SprintController(ISprintRepository sprintRepository)
        {
            _sprintRepository = sprintRepository;
        }

        // GET: api/sprint
        [HttpGet]
        public ActionResult<IEnumerable<SprintDTO>> GetSprints()
        {
            var sprints = _sprintRepository.GetSprints();
            if (sprints == null || !sprints.Any())
            {
                return NoContent(); // 204 No Content ako je lista prazna
            }
            return Ok(sprints); // 200 OK
        }

        // GET: api/sprint/{id}
        [HttpGet("{id}")]
        public ActionResult<SprintDTO> GetSprintById(Guid id)
        {
            var sprint = _sprintRepository.GetSprintById(id);
            if (sprint == null)
            {
                return NotFound(); // 404 Not Found
            }
            return Ok(sprint);
        }

        // POST: api/sprint
        [HttpPost]
        public ActionResult<SprintConfirmationDTO> CreateSprint([FromBody] SprintCreationDTO sprint)
        {
            try
            {
                var confirmation = _sprintRepository.CreateSprint(sprint);

                return Created("", confirmation);
            }
            catch
            {
                return BadRequest(); // 400 Bad Request
            }
        }

        // DELETE: api/sprint/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteSprint(Guid id)
        {
            try
            {
                var existingSprint = _sprintRepository.GetSprintById(id);
                if (existingSprint == null)
                {
                    return NotFound();
                }

                _sprintRepository.DeleteSprint(id);
                return NoContent(); // 204 Success, no content to return
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error while trying to delete.");
            }
        }

        // PUT: api/sprint
        [HttpPut]
        public ActionResult<SprintConfirmationDTO> UpdateSprint([FromBody] Sprint sprint)
        {
            try
            {
                var existingSprint = _sprintRepository.GetSprintById(sprint.Id);
                if (existingSprint == null)
                {
                    return NotFound();
                }

                var updatedSprint = _sprintRepository.UpdateSprint(sprint);
                return Ok(updatedSprint);
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}