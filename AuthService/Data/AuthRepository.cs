using AutoMapper;
using AuthService.Context;
using AuthService.Models;
using AuthService.Models.DTO.AuthDtos;
using AuthService.Models.Enums;

namespace AuthService.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AuthContext _context;
        private readonly IMapper _mapper;

        public AuthRepository(AuthContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public AuthSession CreateSession(Guid userId, string username, string role, string refreshToken, DateTime expiresAt)
        {
            var session = new AuthSession
            {
                AuthId = Guid.NewGuid(),
                UserId = userId,
                Username = username,
                Permission = Enum.TryParse<UserRole>(role, out var parsedRole) ? parsedRole : UserRole.TeamMember,
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IsRevoked = false
            };

            _context.AuthSessions.Add(session);
            _context.SaveChanges();
            return session;
        }

        public AuthSession? GetByRefreshToken(string refreshToken)
        {
            return _context.AuthSessions.FirstOrDefault(s => s.Token == refreshToken);
        }

        public bool RevokeSession(string refreshToken)
        {
            var session = _context.AuthSessions.FirstOrDefault(s => s.Token == refreshToken);
            if (session == null)
                return false;

            session.IsRevoked = true;
            _context.SaveChanges();
            return true;
        }

        public int RevokeAllSessionsForUser(Guid userId)
        {
            var sessions = _context.AuthSessions.Where(s => s.UserId == userId && !s.IsRevoked).ToList();
            foreach (var session in sessions)
            {
                session.IsRevoked = true;
            }
            _context.SaveChanges();
            return sessions.Count;
        }

        public IEnumerable<AuthSessionDto> GetSessionsForUser(Guid userId)
        {
            var sessions = _context.AuthSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToList();

            var result = new List<AuthSessionDto>();
            foreach (var session in sessions)
            {
                result.Add(_mapper.Map<AuthSessionDto>(session));
            }

            return result;
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }
    }
}
