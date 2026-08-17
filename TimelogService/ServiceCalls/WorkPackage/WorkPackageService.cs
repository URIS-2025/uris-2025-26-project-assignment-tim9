using System.Text.Json;
using TimelogService.Models.DTO.WorkPackage;

namespace TimelogService.ServiceCalls.WorkPackage
{
    public class WorkPackageService : IWorkPackageService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;

        public WorkPackageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WorkPackageDTO?> GetWorkPackageByIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/workpackage/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<WorkPackageDTO>(content, JsonOptions);
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
