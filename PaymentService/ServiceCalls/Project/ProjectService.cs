using System.Net;
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

        //token pozivaoca dodaje AuthForwardingHandler, pa se ovde ne prosledjuje rucno
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

        public async Task<ProjectMembershipResult> CheckMembershipAsync(Guid projectId, Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/projectmember/project/{projectId}");

                //projekat bez ijednog clana
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return new ProjectMembershipResult(ProjectMembershipStatus.NotMember);
                }

                if (!response.IsSuccessStatusCode)
                {
                    return new ProjectMembershipResult(ProjectMembershipStatus.ServiceUnavailable);
                }

                var content = await response.Content.ReadAsStringAsync();
                var members = JsonSerializer.Deserialize<List<ProjectMemberDTO>>(content, JsonOptions)
                    ?? new List<ProjectMemberDTO>();

                var isActiveMember = members.Any(m => m.UserId == userId && m.Status);

                return new ProjectMembershipResult(
                    isActiveMember ? ProjectMembershipStatus.Member : ProjectMembershipStatus.NotMember);
            }
            //nedostupan servis ne sme da blokira rad - odluku donosi samo izricit odgovor
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
    }
}
