using AutoMapper;
using UserService.Models;
using UserService.Models.DTO.UserDtos;

namespace UserService.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<User, UserConfirmationDto>();
            CreateMap<UserActivityLog, UserActivityLogDto>();
        }
    }
}
