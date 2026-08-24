using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using AttachmentService.Models.DTO.WorkPackage;

namespace AttachmentService.ServiceCalls.WorkPackage
{
    public class TaskService : ITaskService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;

        public TaskService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TaskLookupResult> GetTaskByIdAsync(Guid id, string? bearerToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/task/{id}");
                if (!string.IsNullOrEmpty(bearerToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                }

                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new TaskLookupResult(TaskLookupStatus.NotFound, null);
                }

                if (!response.IsSuccessStatusCode)
                {
                    return new TaskLookupResult(TaskLookupStatus.ServiceUnavailable, null);
                }

                var content = await response.Content.ReadAsStringAsync();
                var task = JsonSerializer.Deserialize<TaskDTO>(content, JsonOptions);
                return new TaskLookupResult(TaskLookupStatus.Found, task);
            }
            catch (HttpRequestException)
            {
                return new TaskLookupResult(TaskLookupStatus.ServiceUnavailable, null);
            }
            catch (TaskCanceledException)
            {
                return new TaskLookupResult(TaskLookupStatus.ServiceUnavailable, null);
            }
        }
    }
}
