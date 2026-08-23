namespace WorkPackageService.ServiceCalls.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(HttpClient httpClient, ILogger<NotificationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        
        public async Task<bool> SendNotificationAsync(Guid userId, string message, string type)
        {
            try
            {
                var payload = new { userId, message, type };
                var response = await _httpClient.PostAsJsonAsync("/notifications", payload);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Notification service returned {StatusCode} for user {UserId}, type {Type}.",
                        response.StatusCode, userId, type);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to user {UserId}, type {Type}.", userId, type);
                return false;
            }
        }
    }
}
