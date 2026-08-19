using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectService.Data;
using ProjectService.Models;
using ProjectService.Models.DTO.ProjectMemberDtos;
using ProjectService.ServiceCalls.User;

namespace ProjectService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectMemberController : ControllerBase
    {
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public ProjectMemberController(IProjectMemberRepository projectMemberRepository, IUserService userService, IMapper mapper)
        {
            _projectMemberRepository = projectMemberRepository;
            _userService = userService;
            _mapper = mapper;
        }

        private async Task EnrichWithUserInfoAsync(ProjectMemberDto member)
        {
            var user = await _userService.GetUserByIdAsync(member.UserId);
            if (user != null)
            {
                member.Username = user.Username;
                member.Role = user.Role;
            }
        }

        // GET svi clanovi
        [HttpGet]
        [HttpHead]
        public async Task<ActionResult<IEnumerable<ProjectMemberDto>>> GetProjectMembers()
        {
            var members = _projectMemberRepository.GetProjectMembers();
            if (members == null || !members.Any())
                return NoContent();

            var memberList = members.ToList();
            foreach (var member in memberList)
                await EnrichWithUserInfoAsync(member);

            return Ok(memberList);
        }

        // GET clanovi po projektu
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<ProjectMemberDto>>> GetMembersByProjectId(Guid projectId)
        {
            var members = _projectMemberRepository.GetMembersByProjectId(projectId);
            if (members == null || !members.Any())
                return NoContent();

            var memberList = members.ToList();
            foreach (var member in memberList)
                await EnrichWithUserInfoAsync(member);

            return Ok(memberList);
        }

        // GET clan po ID
        [HttpGet("{projectMemberId}")]
        public async Task<ActionResult<ProjectMemberDto>> GetProjectMemberById(Guid projectMemberId)
        {
            var member = _projectMemberRepository.GetProjectMemberById(projectMemberId);
            if (member == null)
                return NotFound($"ProjectMember with ID {projectMemberId} not found.");

            await EnrichWithUserInfoAsync(member);

            return Ok(member);
        }

        // POST kreiraj clana
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProjectMemberConfirmationDto>> CreateProjectMember([FromBody] ProjectMemberCreationDto projectMemberDto)
        {
            var user = await _userService.GetUserByIdAsync(projectMemberDto.UserId);
            if (user == null)
                return BadRequest("User with given ID does not exist.");

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
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProjectMemberConfirmationDto>> UpdateProjectMember([FromBody] ProjectMemberUpdateDto projectMemberDto)
        {
            var user = await _userService.GetUserByIdAsync(projectMemberDto.UserId);
            if (user == null)
                return BadRequest("User with given ID does not exist.");

            try
            {
                var updated = _projectMemberRepository.UpdateProjectMember(projectMemberDto);
                if (updated == null)
                    return NotFound($"ProjectMember with ID {projectMemberDto.ProjectMemberId} not found.");
                return Ok(updated);
            }
            catch
            {
                return BadRequest();
            }
        }

        // DELETE obrisi clana
        [HttpDelete("{projectMemberId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteProjectMember(Guid projectMemberId)
        {
            try
            {
                var member = _projectMemberRepository.GetProjectMemberById(projectMemberId);
                if (member == null)
                    return NotFound($"ProjectMember with ID {projectMemberId} not found.");
                _projectMemberRepository.DeleteProjectMember(projectMemberId);
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
