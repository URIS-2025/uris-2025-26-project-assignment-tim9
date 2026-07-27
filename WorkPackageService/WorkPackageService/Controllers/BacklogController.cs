using Microsoft.AspNetCore.Mvc;
using WorkPackageService.Data;
using WorkPackageService.Models.DTO.BacklogDTOs;

namespace WorkPackageService.Controllers
{
    [ApiController]
    public class BacklogController : ControllerBase
    {
        private readonly IBacklogRepository _repository;

        public BacklogController(IBacklogRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("projects/{projectId}/backlog")]
        public ActionResult<BacklogDisplayDTO> CreateBacklog(Guid projectId, [FromBody] BacklogCreateDTO dto)
        {
            // ProjectId dolazi iz rute - isti razlog za Clear()+TryValidateModel kao u WorkPackageController.
            dto.ProjectId = projectId;
            ModelState.Clear();
            if (!TryValidateModel(dto)) return BadRequest(ModelState);

            var created = _repository.Add(dto);
            if (!_repository.SaveChanges())
            {
                return StatusCode(500, "Doslo je do greske prilikom cuvanja Backlog-a.");
            }

            return CreatedAtAction(nameof(GetBacklogById), new { id = created.BacklogId }, created);
        }

        [HttpGet("projects/{projectId}/backlog")]
        public ActionResult<IEnumerable<BacklogDisplayDTO>> GetBacklogsForProject(Guid projectId)
        {
            return Ok(_repository.GetByProjectId(projectId));
        }

        [HttpGet("backlog/{id}")]
        public ActionResult<BacklogDisplayDTO> GetBacklogById(Guid id)
        {
            var backlog = _repository.GetById(id);
            if (backlog == null) return NotFound();

            return Ok(backlog);
        }

        [HttpPut("backlog/{id}")]
        public ActionResult<BacklogDisplayDTO> UpdateBacklog(Guid id, [FromBody] BacklogUpdateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = _repository.Update(id, dto);
            if (updated == null) return NotFound();

            if (!_repository.SaveChanges())
            {
                return StatusCode(500, "Doslo je do greske prilikom cuvanja izmena.");
            }

            return Ok(updated);
        }

        [HttpDelete("backlog/{id}")]
        public IActionResult DeleteBacklog(Guid id)
        {
            var deleted = _repository.Delete(id);
            if (!deleted) return NotFound();

            if (!_repository.SaveChanges())
            {
                return StatusCode(500, "Doslo je do greske prilikom brisanja.");
            }

            return NoContent();
        }
    }
}
