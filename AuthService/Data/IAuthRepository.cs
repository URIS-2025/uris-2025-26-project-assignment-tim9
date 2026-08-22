using AuthService.Models;
using AuthService.Models.DTO.AuthDtos;

namespace AuthService.Data
{
    public interface IAuthRepository
    {
        AuthSession CreateSession(Guid userId, string username, string role, string refreshToken, DateTime expiresAt);
        AuthSession? GetByRefreshToken(string refreshToken);
        bool RevokeSession(string refreshToken);
        int RevokeAllSessionsForUser(Guid userId);
        IEnumerable<AuthSessionDto> GetSessionsForUser(Guid userId);
        bool SaveChanges();
    }
}
