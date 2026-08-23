using System.Net;
using System.Net.Http.Headers;
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

        public async Task<ProjectMembershipResult> CheckMembershipAsync(Guid projectId, Guid userId, string? bearerToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/projectmember/project/{projectId}");
                if (!string.IsNullOrEmpty(bearerToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                }

                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return new ProjectMembershipResult(ProjectMembershipStatus.NotMember);
                }

                if (!response.IsSuccessStatusCode)
                {
                    return new ProjectMembershipResult(ProjectMembershipStatus.ServiceUnavailable);
                }

                var content = await response.Content.ReadAsStringAsync();
                var members = JsonSerializer.Deserialize<List<ProjectMemberDTO>>(content, JsonOptions) ?? new List<ProjectMemberDTO>();

                var isActiveMember = members.Any(m => m.UserId == userId && m.Status);
                return new ProjectMembershipResult(isActiveMember ? ProjectMembershipStatus.Member : ProjectMembershipStatus.NotMember);
            }
            catch (HttpRequestException)
            {
                return new ProjectMembershipResult(ProjectMembershipStatus.ServiceUnavailable);
            }
            catch (TaskCanceledException)
            {
                return new ProjectMembershipResult(ProjectMembershipStatus.ServiceUnavailable);
            }
            catch (JsonException)
            {
                return new ProjectMembershipResult(ProjectMembershipStatus.ServiceUnavailable);
            }
        }

        public async Task<ProjectExistsResult> CheckProjectExistsAsync(Guid projectId, string? bearerToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/project/{projectId}");
                if (!string.IsNullOrEmpty(bearerToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                }

                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new ProjectExistsResult(ProjectExistsStatus.NotFound);
                }

                if (!response.IsSuccessStatusCode)
                {
                    return new ProjectExistsResult(ProjectExistsStatus.ServiceUnavailable);
                }

                return new ProjectExistsResult(ProjectExistsStatus.Exists);
            }
            catch (HttpRequestException)
            {
                return new ProjectExistsResult(ProjectExistsStatus.ServiceUnavailable);
            }
            catch (TaskCanceledException)
            {
                return new ProjectExistsResult(ProjectExistsStatus.ServiceUnavailable);
            }
        }
    }
}
