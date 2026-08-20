using AutoMapper;
using AuthService.Models;
using AuthService.Models.DTO.AuthDtos;

namespace AuthService.Profiles
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            CreateMap<AuthSession, AuthSessionDto>();
        }
    }
}
