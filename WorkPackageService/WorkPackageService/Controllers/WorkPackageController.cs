using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkPackageService.Data;
using WorkPackageService.Exceptions;
using WorkPackageService.Models.DTO.WorkPackageDTOs;

namespace WorkPackageService.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WorkPackageController : ControllerBase
    {
        private readonly IWorkPackageRepository _workPackageRepository;
        private readonly IMapper _mapper;

        public WorkPackageController(IWorkPackageRepository workPackageRepository, IMapper mapper)
        {
            _workPackageRepository = workPackageRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [HttpHead]
        public ActionResult<IEnumerable<WorkPackageDisplayDTO>> GetWorkPackages()
        {
            var workPackages = _workPackageRepository.GetAll();
            if (workPackages == null || !workPackages.Any())
                return NoContent();
            return Ok(workPackages);
        }

        [HttpGet("{id}")]
        public ActionResult<WorkPackageDisplayDTO> GetWorkPackageById(Guid id)
        {
            var workPackage = _workPackageRepository.GetById(id);
            if (workPackage == null) return NotFound();
            return Ok(workPackage);
        }

        [HttpGet("project/{projectId}")]
        public ActionResult<IEnumerable<WorkPackageDisplayDTO>> GetWorkPackagesByProject(Guid projectId)
        {
            var workPackages = _workPackageRepository.GetByProjectId(projectId);
            if (workPackages == null || !workPackages.Any())
                return NoContent();
            return Ok(workPackages);
        }

        [HttpPost]
        public ActionResult<WorkPackageDisplayDTO> CreateWorkPackage([FromBody] WorkPackageCreateDTO dto)
        {
            try
            {
                var created = _workPackageRepository.Add(dto);
                return Created("", created);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPut]
        public ActionResult<WorkPackageDisplayDTO> UpdateWorkPackage([FromBody] WorkPackageUpdateDTO dto)
        {
            try
            {
                var updated = _workPackageRepository.Update(dto.Id, dto);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteWorkPackage(Guid id)
        {
            try
            {
                var deleted = _workPackageRepository.Delete(id);
                if (!deleted) return NotFound();
                return NoContent();
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedOperationException)
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Delete Error");
            }
        }

        [HttpOptions]
        public IActionResult GetWorkPackageOptions()
        {
            Response.Headers["Allow"] = "GET, POST, PUT, DELETE";
            return Ok();
        }
    }
}
