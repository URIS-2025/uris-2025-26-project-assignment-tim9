using Newtonsoft.Json;
using ProjectService.Models.DTO.UserDtos;

namespace ProjectService.ServiceCalls.User
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserService> _logger;

        public UserService(HttpClient httpClient, ILogger<UserService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<UserProjectDto> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/user/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "User service returned {StatusCode} for user {UserId}.",
                        response.StatusCode, userId);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserProjectDto>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch user {UserId} from User service.", userId);
                return null;
            }
        }
    }
}