using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthService.Data;
using AuthService.Models.DTO.AuthDtos;
using AuthService.ServiceCalls.User;
using AuthService.Services;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AuthController(
            IAuthRepository authRepository,
            IUserService userService,
            ITokenService tokenService,
            IConfiguration configuration)
        {
            _authRepository = authRepository;
            _userService = userService;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        // POST prijava korisnika
        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            var validation = await _userService.ValidateCredentialsAsync(loginDto.Username, loginDto.Password);

            if (validation == null)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "User service is unavailable.");

            if (!validation.IsValid)
                return Unauthorized("Invalid username or password.");

            if (!validation.IsActive)
                return Unauthorized("User account is deactivated.");

            var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(validation.UserId, validation.Username, validation.Role);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshDays = int.TryParse(_configuration["Jwt:RefreshTokenExpirationDays"], out var days) ? days : 7;

            _authRepository.CreateSession(
                validation.UserId,
                validation.Username,
                validation.Role,
                refreshToken,
                DateTime.UtcNow.AddDays(refreshDays));

            return Ok(new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                Username = validation.Username,
                Role = validation.Role
            });
        }

        // POST osvezavanje access tokena
        [HttpPost("refresh")]
        public ActionResult<TokenResponseDto> Refresh([FromBody] RefreshDto refreshDto)
        {
            var session = _authRepository.GetByRefreshToken(refreshDto.RefreshToken);

            if (session == null || session.IsRevoked || session.ExpiresAt < DateTime.UtcNow)
                return Unauthorized("Refresh token is invalid or expired.");

            var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(
                session.UserId, session.Username, session.Permission.ToString());

            return Ok(new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = session.Token,
                ExpiresAt = expiresAt,
                Username = session.Username,
                Role = session.Permission.ToString()
            });
        }

        // POST odjava (invalidira jednu sesiju)
        [HttpPost("logout")]
        public IActionResult Logout([FromBody] RefreshDto refreshDto)
        {
            var success = _authRepository.RevokeSession(refreshDto.RefreshToken);
            if (!success)
                return NotFound();
            return NoContent();
        }

        // POST poništi sve sesije korisnika [Interni poziv UserService-a]
        [HttpPost("revoke/{userId}")]
        public IActionResult RevokeAllSessions(Guid userId)
        {
            _authRepository.RevokeAllSessionsForUser(userId);
            return NoContent();
        }

        // GET sve sesije korisnika [Admin]
        [Authorize(Roles = "Admin")]
        [HttpGet("sessions/{userId}")]
        public ActionResult<IEnumerable<AuthSessionDto>> GetSessions(Guid userId)
        {
            var sessions = _authRepository.GetSessionsForUser(userId);
            if (sessions == null || !sessions.Any())
                return NoContent();
            return Ok(sessions);
        }

        // OPTIONS
        [HttpOptions]
        public IActionResult GetAuthOptions()
        {
            Response.Headers.Append("Allow", "POST, GET");
            return Ok();
        }
    }
}
