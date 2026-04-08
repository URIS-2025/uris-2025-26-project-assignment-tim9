using Newtonsoft.Json;
using TimelogService.Models.DTO;
using TimelogService.Models.DTO.WorkPackage;

namespace TimelogService.ServiceCalls.WorkPackage
{
    public class WorkPackageService : IWorkPackageService
    {
        private readonly IConfiguration _configuration;

        public WorkPackageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public WorkPackageDTO GetWorkPackageById(Guid id)
        {
            using (HttpClient client = new HttpClient())
            {
                string baseUrl = _configuration["Services:WorkPackageService"];
                Uri url = new Uri($"{baseUrl}api/workpackage/{id}");

                var response = client.GetAsync(url).Result;

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<WorkPackageDTO>(content);
            }
        }
    }
}