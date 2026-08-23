using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkPackageService.Data;
using WorkPackageService.Models.DTO.DependencyDTOs;

namespace WorkPackageService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DependencyController : ControllerBase
    {
        private readonly IDependencyRepository _dependencyRepository;
        private readonly IMapper _mapper;

        public DependencyController(IDependencyRepository dependencyRepository, IMapper mapper)
        {
            _dependencyRepository = dependencyRepository;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet]
        [HttpHead]
        public ActionResult<IEnumerable<DependencyDisplayDTO>> GetDependencies()
        {
            var dependencies = _dependencyRepository.GetAll();
            if (dependencies == null || !dependencies.Any())
                return NoContent();
            return Ok(dependencies);
        }

        [Authorize]
        [HttpGet("{id}")]
        public ActionResult<DependencyDisplayDTO> GetDependencyById(Guid id)
        {
            var dependency = _dependencyRepository.GetById(id);
            if (dependency == null) return NotFound();
            return Ok(dependency);
        }

        [Authorize]
        [HttpGet("task/{taskId}")]
        public ActionResult<IEnumerable<DependencyDisplayDTO>> GetDependenciesByTask(Guid taskId)
        {
            var dependencies = _dependencyRepository.GetByTaskId(taskId);
            if (dependencies == null || !dependencies.Any())
                return NoContent();
            return Ok(dependencies);
        }

        [Authorize(Roles = "ProjectManager,Admin")]
        [HttpPost]
        public ActionResult<DependencyDisplayDTO> CreateDependency([FromBody] DependencyCreateDTO dto)
        {
            try
            {
                var created = _dependencyRepository.Add(dto);
                if (created == null)
                {
                    return BadRequest("Task ne moze blokirati sam sebe.");
                }
                return Created("", created);
            }
            catch
            {
                return BadRequest();
            }
        }

        [Authorize(Roles = "ProjectManager,Admin")]
        [HttpPut]
        public ActionResult<DependencyDisplayDTO> UpdateDependency([FromBody] DependencyUpdateDTO dto)
        {
            try
            {
                var updated = _dependencyRepository.Update(dto.Id, dto);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch
            {
                return BadRequest();
            }
        }

        [Authorize(Roles = "ProjectManager,Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteDependency(Guid id)
        {
            try
            {
                var deleted = _dependencyRepository.Delete(id);
                if (!deleted) return NotFound();
                return NoContent();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Delete Error");
            }
        }

        [HttpOptions]
        public IActionResult GetDependencyOptions()
        {
            Response.Headers["Allow"] = "GET, POST, PUT, DELETE";
            return Ok();
        }
    }
}