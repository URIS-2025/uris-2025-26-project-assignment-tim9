namespace UserService.ServiceCalls.Auth
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthService> _logger;

        public AuthService(HttpClient httpClient, ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task RevokeSessionsAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/auth/revoke/{userId}", null);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Auth service returned {StatusCode} while revoking sessions for user {UserId}.",
                        response.StatusCode, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke sessions for user {UserId} via Auth service.", userId);
            }
        }
    }
}
