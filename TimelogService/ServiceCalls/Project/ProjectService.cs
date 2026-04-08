using Newtonsoft.Json;
using TimelogService.Models.DTO;
using TimelogService.Models.DTO.Project;

namespace TimelogService.ServiceCalls.Project
{
    public class ProjectService : IProjectService
    {
        private readonly IConfiguration _configuration;

        public ProjectService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ProjectDTO GetProjectById(Guid id)
        {
            using (HttpClient client = new HttpClient())
            {
                string baseUrl = _configuration["Services:ProjectService"];
                Uri url = new Uri($"{baseUrl}api/project/{id}");

                var response = client.GetAsync(url).Result;
                if (!response.IsSuccessStatusCode) return null;

                var content = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<ProjectDTO>(content);
            }
        }
    }
}