using Microsoft.AspNetCore.Mvc;
using WorkPackageService.Data;
using WorkPackageService.Models.DTO.DependencyDTOs;

namespace WorkPackageService.Controllers
{
    [ApiController]
    public class DependencyController : ControllerBase
    {
        private readonly IDependencyRepository _repository;

        public DependencyController(IDependencyRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("tasks/{taskId}/dependencies")]
        public ActionResult<DependencyDisplayDTO> CreateDependency(Guid taskId, [FromBody] DependencyCreateDTO dto)
        {
            // TaskId dolazi iz rute - isti razlog za Clear()+TryValidateModel kao u ostalim
            // kontrolerima. Ovde je narocito bitno jer [NotEqualToProperty] poredi BlockerTaskId
            // bas protiv ovog TaskId polja - mora imati ispravnu (route) vrednost pre validacije.
            dto.TaskId = taskId;
            ModelState.Clear();
            if (!TryValidateModel(dto)) return BadRequest(ModelState);

            var created = _repository.Add(dto);
            if (created == null)
            {
                // Odbrana u dubinu - [NotEqualToProperty] na DTO-u vec hvata ovo pre nego sto
                // stigne do repository-ja, ali repo i dalje proverava isto poslovno pravilo.
                return BadRequest("Task ne moze blokirati sam sebe.");
            }

            return CreatedAtAction(nameof(GetDependencyById), new { id = created.DependencyId }, created);
        }

        [HttpGet("tasks/{taskId}/dependencies")]
        public ActionResult<IEnumerable<DependencyDisplayDTO>> GetDependenciesForTask(Guid taskId)
        {
            return Ok(_repository.GetByTaskId(taskId));
        }

        [HttpGet("dependencies/{id}")]
        public ActionResult<DependencyDisplayDTO> GetDependencyById(Guid id)
        {
            var dependency = _repository.GetById(id);
            if (dependency == null) return NotFound();

            return Ok(dependency);
        }

        [HttpDelete("dependencies/{id}")]
        public IActionResult DeleteDependency(Guid id)
        {
            var deleted = _repository.Delete(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
