using Microsoft.AspNetCore.Mvc;
using WorkPackageService.Data;
using WorkPackageService.Models.DTO.WorkPackageDTOs;

namespace WorkPackageService.Controllers
{
    [ApiController]
    public class WorkPackageController : ControllerBase
    {
        private readonly IWorkPackageRepository _repository;

        public WorkPackageController(IWorkPackageRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("projects/{projectId}/work-packages")]
        public ActionResult<WorkPackageDisplayDTO> CreateWorkPackage(Guid projectId, [FromBody] WorkPackageCreateDTO dto)
        {
            // ProjectId dolazi iz rute, ne iz tela zahteva - postavi ga pre validacije
            // i eksplicitno revalidiraj, jer je ModelState vec popunjen (od strane model
            // bindera) na osnovu originalne vrednosti iz tela (koja bi bila Guid.Empty).
            dto.ProjectId = projectId;
            ModelState.Clear();
            if (!TryValidateModel(dto)) return BadRequest(ModelState);

            var created = _repository.Add(dto);

            return CreatedAtAction(nameof(GetWorkPackageById), new { id = created.WorkPackageId }, created);
        }

        [HttpGet("projects/{projectId}/work-packages")]
        public ActionResult<IEnumerable<WorkPackageDisplayDTO>> GetWorkPackagesForProject(Guid projectId)
        {
            return Ok(_repository.GetByProjectId(projectId));
        }

        [HttpGet("work-packages/{id}")]
        public ActionResult<WorkPackageDisplayDTO> GetWorkPackageById(Guid id)
        {
            var workPackage = _repository.GetById(id);
            if (workPackage == null) return NotFound();

            return Ok(workPackage);
        }

        [HttpPut("work-packages/{id}")]
        public ActionResult<WorkPackageDisplayDTO> UpdateWorkPackage(Guid id, [FromBody] WorkPackageUpdateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = _repository.Update(id, dto);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        [HttpDelete("work-packages/{id}")]
        public IActionResult DeleteWorkPackage(Guid id)
        {
            var deleted = _repository.Delete(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
