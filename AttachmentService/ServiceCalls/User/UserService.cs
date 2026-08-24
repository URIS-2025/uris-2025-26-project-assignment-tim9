using System.Text.Json;
using AttachmentService.Models.DTO.User;

namespace AttachmentService.ServiceCalls.User
{
    public class UserService : IUserService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserInfoDTO?> GetUserInfoAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/user/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<UserInfoDTO>(content, JsonOptions);
            }
            catch (HttpRequestException)
            {
                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }
    }
}
