using System.Net;
using System.Text.Json;
using SprintService.Models.DTO.Project;

namespace SprintService.ServiceCalls.Project
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

        public async Task<MilestoneDTO?> GetProjectByIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/milestone/project/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    return null;
                }

                var milestones = JsonSerializer.Deserialize<List<ProjectMilestoneDTO>>(content, JsonOptions);
                if (milestones is null || milestones.Count == 0)
                {
                    return null;
                }

                var next = milestones
                    .Where(m => m.ExpectedDate >= DateTime.UtcNow)
                    .OrderBy(m => m.ExpectedDate)
                    .FirstOrDefault()
                    ?? milestones.OrderByDescending(m => m.ExpectedDate).First();

                return new MilestoneDTO { MilestoneID = next.MilestoneId, ExpectedDate = next.ExpectedDate };
            }
            catch (HttpRequestException)
            {
                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public async Task<ProjectExistence> CheckProjectExistsAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/project/{id}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return ProjectExistence.NotFound;
                }
                return response.IsSuccessStatusCode ? ProjectExistence.Exists : ProjectExistence.Unknown;
            }
            catch (HttpRequestException)
            {
                return ProjectExistence.Unknown;
            }
            catch (TaskCanceledException)
            {
                return ProjectExistence.Unknown;
            }
        }

        public async Task<List<Guid>> GetProjectIdsForUserAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/project/user/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    return new List<Guid>();
                }

                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    return new List<Guid>();
                }

                var projects = JsonSerializer.Deserialize<List<ProjectSummaryDTO>>(content, JsonOptions);
                return projects?.Select(p => p.ProjectId).ToList() ?? new List<Guid>();
            }
            catch (HttpRequestException)
            {
                return new List<Guid>();
            }
            catch (TaskCanceledException)
            {
                return new List<Guid>();
            }
            catch (JsonException)
            {
                return new List<Guid>();
            }
        }
    }
}
