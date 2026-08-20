namespace UserService.ServiceCalls.Auth
{
    public interface IAuthService
    {
        Task RevokeSessionsAsync(Guid userId);
    }
}
