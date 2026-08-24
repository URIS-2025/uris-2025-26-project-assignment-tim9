namespace AuthService.Services
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) GenerateAccessToken(Guid userId, string username, string role);
        string GenerateRefreshToken();
    }
}
