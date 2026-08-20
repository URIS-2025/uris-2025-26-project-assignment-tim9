using UserService.Models.DTO.AuthDtos;
using UserService.Models.DTO.UserDtos;
using UserService.Models.Enums;

namespace UserService.Data
{
    public interface IUserRepository
    {
        IEnumerable<UserDto> GetUsers(string? search, UserRole? role, bool? isActive);
        UserDto? GetUserById(Guid userId);
        UserDto? GetUserByUsername(string username);
        bool UsernameExists(string username);
        bool EmailExists(string email);
        UserConfirmationDto CreateUser(UserCreationDto userDto);
        UserConfirmationDto? UpdateUser(Guid userId, UserUpdateDto userDto);
        bool SetActiveStatus(Guid userId, bool isActive, Guid performedBy);
        Task<bool> ChangeRole(RoleUpdateDto roleDto);
        void DeleteUser(Guid userId);
        UserAuthVo ValidateCredentials(string username, string password);
        IEnumerable<UserActivityLogDto> GetActivityLog(Guid userId);
        bool SaveChanges();
    }
}
