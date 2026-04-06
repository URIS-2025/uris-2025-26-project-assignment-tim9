using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProjectService.Data;
using ProjectService.Models;
using ProjectService.Models.DTO.RequirementsDtos;

namespace ProjectService.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RequirementsController : ControllerBase
    {
        private readonly IRequirementsRepository _requirementsRepository;
        private readonly IMapper _mapper;

        public RequirementsController(IRequirementsRepository requirementsRepository, IMapper mapper)
        {
            _requirementsRepository = requirementsRepository;
            _mapper = mapper;
        }

        // GET svi zahtevi
        [HttpGet]
        [HttpHead]
        public ActionResult<IEnumerable<RequirementsDto>> GetRequirements()
        {
            var requirements = _requirementsRepository.GetRequirements();
            if (requirements == null || !requirements.Any())
                return NoContent();
            return Ok(requirements);
        }

        // GET zahtevi po projektu
        [HttpGet("project/{ProjectId}")]
        public ActionResult<IEnumerable<RequirementsDto>> GetRequirementsByProjectId(Guid ProjectId)
        {
            var requirements = _requirementsRepository.GetRequirementsByProjectId(ProjectId);
            if (requirements == null || !requirements.Any())
                return NoContent();
            return Ok(requirements);
        }

        // GET zahtev po ID
        [HttpGet("{RequirementId}")]
        public ActionResult<RequirementsDto> GetRequirementById(Guid RequirementId)
        {
            var requirement = _requirementsRepository.GetRequirementById(RequirementId);
            if (requirement == null)
                return NotFound();
            return Ok(requirement);
        }

        // POST kreiraj zahtev
        [HttpPost]
        public ActionResult<RequirementsConfirmationDto> CreateRequirement([FromBody] RequirementsCreationDto requirementDto)
        {
            try
            {
                var requirement = _requirementsRepository.CreateRequirement(requirementDto);
                return Created("", requirement);
            }
            catch
            {
                return BadRequest();
            }
        }

        // PUT azuriraj zahtev
        [HttpPut]
        public ActionResult<RequirementsConfirmationDto> UpdateRequirement([FromBody] Requirements requirement)
        {
            try
            {
                var existing = _requirementsRepository.GetRequirementById(requirement.RequirementId);
                if (existing == null)
                    return NotFound();
                var updated = _requirementsRepository.UpdateRequirement(requirement);
                return Ok(updated);
            }
            catch
            {
                return BadRequest();
            }
        }

        // DELETE obrisi zahtev
        [HttpDelete("{RequirementId}")]
        public IActionResult DeleteRequirement(Guid RequirementId)
        {
            try
            {
                var requirement = _requirementsRepository.GetRequirementById(RequirementId);
                if (requirement == null)
                    return NotFound();
                _requirementsRepository.DeleteRequirement(RequirementId);
                return NoContent();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Delete Error");
            }
        }

        // OPTIONS
        [HttpOptions]
        public IActionResult GetRequirementsOptions()
        {
            Response.Headers.Add("Allow", "GET, POST, PUT, DELETE");
            return Ok();
        }
    }
}
