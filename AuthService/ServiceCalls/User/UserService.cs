using Newtonsoft.Json;
using AuthService.Models.DTO.UserDtos;

namespace AuthService.ServiceCalls.User
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

        public async Task<UserAuthVo?> ValidateCredentialsAsync(string username, string password)
        {
            try
            {
                var payload = new CredentialsValidationDto { Username = username, Password = password };
                var content = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/user/credentials/validate", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return JsonConvert.DeserializeObject<UserAuthVo>(responseContent);
                }

                _logger.LogWarning(
                    "User service returned {StatusCode} while validating credentials for {Username}.",
                    response.StatusCode, username);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate credentials for {Username} via User service.", username);
                return null;
            }
        }
    }
}
