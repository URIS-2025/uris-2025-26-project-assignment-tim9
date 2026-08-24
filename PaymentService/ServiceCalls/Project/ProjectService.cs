using System.Text.Json;
using PaymentService.Models.DTO.Project;

namespace PaymentService.ServiceCalls.Project
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

        public async Task<ProjectInfoDTO?> GetProjectInfoAsync(Guid projectId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/project/{projectId}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ProjectInfoDTO>(content, JsonOptions);
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
