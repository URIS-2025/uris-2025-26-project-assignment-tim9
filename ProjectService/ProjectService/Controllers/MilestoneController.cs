using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectService.Data;
using ProjectService.Models.DTO.MilestoneDtos;

namespace ProjectService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MilestoneController : ControllerBase
    {
        private readonly IMilestoneRepository _milestoneRepository;
        private readonly IMapper _mapper;

        public MilestoneController(IMilestoneRepository milestoneRepository, IMapper mapper)
        {
            _milestoneRepository = milestoneRepository;
            _mapper = mapper;
        }

        // GET svi milestone-ovi
        [HttpGet]
        [HttpHead]
        public ActionResult<IEnumerable<MilestoneDto>> GetMilestones()
        {
            var milestones = _milestoneRepository.GetMilestones();
            if (milestones == null || !milestones.Any())
                return NoContent();
            return Ok(milestones);
        }

        // GET milestone-ovi po projektu
        [HttpGet("project/{projectId}")]
        public ActionResult<IEnumerable<MilestoneDto>> GetMilestonesByProjectId(Guid projectId)
        {
            var milestones = _milestoneRepository.GetMilestonesByProjectId(projectId);
            if (milestones == null || !milestones.Any())
                return NoContent();
            return Ok(milestones);
        }

        // GET milestone po ID
        [HttpGet("{milestoneId}")]
        public ActionResult<MilestoneDto> GetMilestoneById(Guid milestoneId)
        {
            var milestone = _milestoneRepository.GetMilestoneById(milestoneId);
            if (milestone == null)
                return NotFound();
            return Ok(milestone);
        }

        // POST
        [HttpPost]
        [Authorize(Roles = "Admin,ProjectManager")]
        public ActionResult<MilestoneConfirmationDto> CreateMilestone([FromBody] MilestoneCreationDto milestoneDto)
        {
            try
            {
                var milestone = _milestoneRepository.CreateMilestone(milestoneDto);
                return Created("", milestone);
            }
            catch
            {
                return BadRequest();
            }
        }

        // PUT
        [HttpPut]
        [Authorize(Roles = "Admin,ProjectManager")]
        public ActionResult<MilestoneConfirmationDto> UpdateMilestone([FromBody] MilestoneUpdateDto milestoneDto)
        {
            try
            {
                var updated = _milestoneRepository.UpdateMilestone(milestoneDto);
                if (updated == null)
                    return NotFound();
                return Ok(updated);
            }
            catch
            {
                return BadRequest();
            }
        }

        // DELETE
        [HttpDelete("{milestoneId}")]
        [Authorize(Roles = "Admin,ProjectManager")]
        public IActionResult DeleteMilestone(Guid milestoneId)
        {
            try
            {
                var milestone = _milestoneRepository.GetMilestoneById(milestoneId);
                if (milestone == null)
                    return NotFound();
                _milestoneRepository.DeleteMilestone(milestoneId);
                return NoContent();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Delete Error");
            }
        }

        // OPTIONS
        [HttpOptions]
        public IActionResult GetMilestoneOptions()
        {
            Response.Headers.Add("Allow", "GET, POST, PUT, DELETE");
            return Ok();
        }
    }
}
