namespace WorkPackageService.ServiceCalls.Project
{
    public class ProjectService : IProjectService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(HttpClient httpClient, ILogger<ProjectService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<DateTime?> GetProjectDeadlineAsync(Guid projectId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/projects/{projectId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Project service returned {StatusCode} for project {ProjectId}.",
                        response.StatusCode, projectId);
                    return null;
                }
                var result = await response.Content.ReadFromJsonAsync<ProjectDeadlineDTO>();
                return result?.Deadline;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch deadline for project {ProjectId}.", projectId);
                return null;
            }
        }
    }

    public class ProjectDeadlineDTO
    {
        public DateTime Deadline { get; set; }
    }
}

