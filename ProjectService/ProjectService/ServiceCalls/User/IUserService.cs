using ProjectService.Models.DTO.UserDtos;

namespace ProjectService.ServiceCalls.User
{
    public interface IUserService
    {
        Task<UserProjectDto> GetUserByIdAsync(Guid userId);
    }
}
