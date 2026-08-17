using System.Text.Json;
using TimelogService.Models.DTO.Project;

namespace TimelogService.ServiceCalls.Project
{
    public class ProjectService : IProjectService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;

        public ProjectService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserInfoDTO?> GetUserInfoAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/project/users/{userId}");

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
