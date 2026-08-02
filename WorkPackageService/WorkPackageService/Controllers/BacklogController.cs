using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkPackageService.Data;
using WorkPackageService.Models.DTO.BacklogDTOs;

namespace WorkPackageService.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BacklogController : ControllerBase
    {
        private readonly IBacklogRepository _backlogRepository;
        private readonly IMapper _mapper;

        public BacklogController(IBacklogRepository backlogRepository, IMapper mapper)
        {
            _backlogRepository = backlogRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [HttpHead]
        public ActionResult<IEnumerable<BacklogDisplayDTO>> GetBacklogs()
        {
            var backlogs = _backlogRepository.GetAll();
            if (backlogs == null || !backlogs.Any())
                return NoContent();
            return Ok(backlogs);
        }

        [HttpGet("{id}")]
        public ActionResult<BacklogDisplayDTO> GetBacklogById(Guid id)
        {
            var backlog = _backlogRepository.GetById(id);
            if (backlog == null) return NotFound();
            return Ok(backlog);
        }

        [HttpGet("project/{projectId}")]
        public ActionResult<IEnumerable<BacklogDisplayDTO>> GetBacklogsByProject(Guid projectId)
        {
            var backlogs = _backlogRepository.GetByProjectId(projectId);
            if (backlogs == null || !backlogs.Any())
                return NoContent();
            return Ok(backlogs);
        }

        [HttpPost]
        public ActionResult<BacklogDisplayDTO> CreateBacklog([FromBody] BacklogCreateDTO dto)
        {
            try
            {
                var created = _backlogRepository.Add(dto);
                return Created("", created);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPut]
        public ActionResult<BacklogDisplayDTO> UpdateBacklog([FromBody] BacklogUpdateDTO dto)
        {
            try
            {
                var updated = _backlogRepository.Update(dto.Id, dto);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBacklog(Guid id)
        {
            try
            {
                var deleted = _backlogRepository.Delete(id);
                if (!deleted) return NotFound();
                return NoContent();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Delete Error");
            }
        }

        [HttpOptions]
        public IActionResult GetBacklogOptions()
        {
            Response.Headers["Allow"] = "GET, POST, PUT, DELETE";
            return Ok();
        }
    }
}
