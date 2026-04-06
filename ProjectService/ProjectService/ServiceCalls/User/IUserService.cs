using ProjectService.Models.DTO.UserDtos;

namespace ProjectService.ServiceCalls.User
{
    public interface IUserService
    {
        UserProjectDto GetUserById(Guid UserId);
    }
}
