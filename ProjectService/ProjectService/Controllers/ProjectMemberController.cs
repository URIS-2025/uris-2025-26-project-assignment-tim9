using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectService.Data;
using ProjectService.Models;
using ProjectService.Models.DTO.ProjectMemberDtos;

namespace ProjectService.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectMemberController : ControllerBase
    {
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IMapper _mapper;

        public ProjectMemberController(IProjectMemberRepository projectMemberRepository, IMapper mapper)
        {
            _projectMemberRepository = projectMemberRepository;
            _mapper = mapper;
        }

        // GET svi clanovi
        [HttpGet]
        [HttpHead]
        public ActionResult<IEnumerable<ProjectMemberDto>> GetProjectMembers()
        {
            var members = _projectMemberRepository.GetProjectMembers();
            if (members == null || !members.Any())
                return NoContent();
            return Ok(members);
        }

        // GET clanovi po projektu
        [HttpGet("project/{ProjectId}")]
        public ActionResult<IEnumerable<ProjectMemberDto>> GetMembersByProjectId(Guid ProjectId)
        {
            var members = _projectMemberRepository.GetMembersByProjectId(ProjectId);
            if (members == null || !members.Any())
                return NoContent();
            return Ok(members);
        }

        // GET clan po ID
        [HttpGet("{ProjectMemberId}")]
        public ActionResult<ProjectMemberDto> GetProjectMemberById(Guid ProjectMemberId)
        {
            var member = _projectMemberRepository.GetProjectMemberById(ProjectMemberId);
            if (member == null)
                return NotFound();
            return Ok(member);
        }

        // POST kreiraj clana
        [HttpPost]
        public ActionResult<ProjectMemberConfirmationDto> CreateProjectMember([FromBody] ProjectMemberCreationDto projectMemberDto)
        {
            try
            {
                var member = _projectMemberRepository.CreateProjectMember(projectMemberDto);
                return Created("", member);
            }
            catch
            {
                return BadRequest();
            }
        }

        // PUT azuriraj clana
        [HttpPut]
        public ActionResult<ProjectMemberConfirmationDto> UpdateProjectMember([FromBody] ProjectMember projectMember)
        {
            try
            {
                var existing = _projectMemberRepository.GetProjectMemberById(projectMember.ProjectMemberId);
                if (existing == null)
                    return NotFound();
                var updated = _projectMemberRepository.UpdateProjectMember(projectMember);
                return Ok(updated);
            }
            catch
            {
                return BadRequest();
            }
        }

        // DELETE obrisi clana
        [HttpDelete("{ProjectMemberId}")]
        public IActionResult DeleteProjectMember(Guid ProjectMemberId)
        {
            try
            {
                var member = _projectMemberRepository.GetProjectMemberById(ProjectMemberId);
                if (member == null)
                    return NotFound();
                _projectMemberRepository.DeleteProjectMember(ProjectMemberId);
                return NoContent();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Delete Error");
            }
        }

        // OPTIONS
        [HttpOptions]
        public IActionResult GetProjectMemberOptions()
        {
            Response.Headers.Add("Allow", "GET, POST, PUT, DELETE");
            return Ok();
        }
    }
}
