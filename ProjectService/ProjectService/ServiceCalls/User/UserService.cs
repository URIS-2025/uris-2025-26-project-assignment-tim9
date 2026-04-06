using Newtonsoft.Json;
using ProjectService.Models.DTO.UserDtos;

namespace ProjectService.ServiceCalls.User
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;

        public UserService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public UserProjectDto GetUserById(Guid UserId)
        {
            using (HttpClient client = new HttpClient())
            {
                Uri url = new Uri($"{_configuration["Services:UserService"]}api/user/{UserId}");
                var response = client.GetAsync(url).Result;

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<UserProjectDto>(content);
            }
        }
    }
}
