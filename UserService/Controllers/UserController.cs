using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Data;
using UserService.Models.DTO.AuthDtos;
using UserService.Models.DTO.UserDtos;
using UserService.Models.Enums;
using UserService.ServiceCalls.Auth;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public UserController(IUserRepository userRepository, IAuthService authService, IMapper mapper)
        {
            _userRepository = userRepository;
            _authService = authService;
            _mapper = mapper;
        }

        // GET svi korisnici (pretraga po imenu/username/email, filter po ulozi i statusu)
        [HttpGet]
        [HttpHead]
        public ActionResult<IEnumerable<UserDto>> GetUsers(
            [FromQuery] string? search,
            [FromQuery] UserRole? role,
            [FromQuery] bool? isActive)
        {
            var users = _userRepository.GetUsers(search, role, isActive);
            if (users == null || !users.Any())
                return NoContent();
            return Ok(users);
        }

        // GET korisnik po ID-u
        [HttpGet("{userId}")]
        public ActionResult<UserDto> GetUserById(Guid userId)
        {
            var user = _userRepository.GetUserById(userId);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        // GET korisnik po korisnickom imenu
        [HttpGet("username/{username}")]
        public ActionResult<UserDto> GetUserByUsername(string username)
        {
            var user = _userRepository.GetUserByUsername(username);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        // GET audit log korisnika [Admin]
        [Authorize(Roles = "Admin")]
        [HttpGet("{userId}/audit")]
        public ActionResult<IEnumerable<UserActivityLogDto>> GetActivityLog(Guid userId)
        {
            var user = _userRepository.GetUserById(userId);
            if (user == null)
                return NotFound();

            var logs = _userRepository.GetActivityLog(userId);
            return Ok(logs);
        }

        // POST registracija novog korisnika
        [HttpPost]
        public ActionResult<UserConfirmationDto> CreateUser([FromBody] UserCreationDto userDto)
        {
            if (_userRepository.UsernameExists(userDto.Username))
                return BadRequest($"Username '{userDto.Username}' is already taken.");

            if (_userRepository.EmailExists(userDto.Email))
                return BadRequest($"Email '{userDto.Email}' is already registered.");

            try
            {
                var user = _userRepository.CreateUser(userDto);
                return CreatedAtAction(nameof(GetUserById), new { userId = user.UserId }, user);
            }
            catch
            {
                return BadRequest();
            }
        }

        // POST validacija kredencijala [Interni poziv AuthService-a]
        [HttpPost("credentials/validate")]
        public ActionResult<UserAuthVo> ValidateCredentials([FromBody] CredentialsValidationDto credentials)
        {
            var result = _userRepository.ValidateCredentials(credentials.Username, credentials.Password);
            if (!result.IsValid)
                return Unauthorized(result);

            return Ok(result);
        }

        // PUT azuriranje profila korisnika
        [HttpPut("{userId}")]
        public ActionResult<UserConfirmationDto> UpdateUser(Guid userId, [FromBody] UserUpdateDto userDto)
        {
            try
            {
                var updated = _userRepository.UpdateUser(userId, userDto);
                if (updated == null)
                    return NotFound();
                return Ok(updated);
            }
            catch
            {
                return BadRequest();
            }
        }

        // PATCH deaktivacija korisnika [Admin]
        [Authorize(Roles = "Admin")]
        [HttpPatch("{userId}/deactivate")]
        public async Task<IActionResult> DeactivateUser(Guid userId, [FromQuery] Guid performedBy)
        {
            var success = _userRepository.SetActiveStatus(userId, false, performedBy);
            if (!success)
                return NotFound();

            await _authService.RevokeSessionsAsync(userId);
            return NoContent();
        }

        // PATCH aktivacija korisnika [Admin]
        [Authorize(Roles = "Admin")]
        [HttpPatch("{userId}/activate")]
        public IActionResult ActivateUser(Guid userId, [FromQuery] Guid performedBy)
        {
            var success = _userRepository.SetActiveStatus(userId, true, performedBy);
            if (!success)
                return NotFound();
            return NoContent();
        }

        // PATCH promena uloge [Admin]
        [Authorize(Roles = "Admin")]
        [HttpPatch("role")]
        public async Task<IActionResult> ChangeRole([FromBody] RoleUpdateDto roleDto)
        {
            var success = await _userRepository.ChangeRole(roleDto);
            if (!success)
                return BadRequest("Role change is not allowed (user not found or admin cannot revoke own admin rights).");

            // Promena uloge odmah vazi na svim sesijama korisnika
            await _authService.RevokeSessionsAsync(roleDto.UserId);
            return NoContent();
        }

        // DELETE brisanje korisnika [Admin]
        [Authorize(Roles = "Admin")]
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            try
            {
                var user = _userRepository.GetUserById(userId);
                if (user == null)
                    return NotFound();

                _userRepository.DeleteUser(userId);
                await _authService.RevokeSessionsAsync(userId);
                return NoContent();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Delete Error");
            }
        }

        // OPTIONS
        [HttpOptions]
        public IActionResult GetUserOptions()
        {
            Response.Headers.Append("Allow", "GET, POST, PUT, PATCH, DELETE");
            return Ok();
        }
    }
}
