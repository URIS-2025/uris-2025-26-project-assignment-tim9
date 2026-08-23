using AutoMapper;
using WorkPackageService.Models;
using WorkPackageService.Models.DTO.BacklogDTOs;

namespace WorkPackageService.Profiles
{
    public class BacklogProfile : Profile
    {
        public BacklogProfile()
        {
            CreateMap<Backlog, BacklogDisplayDTO>();

            CreateMap<BacklogCreateDTO, Backlog>();

            CreateMap<BacklogUpdateDTO, Backlog>()
                .ForMember(dest => dest.Name, opt => opt.Condition(src => src.Name != null))
                .ForMember(dest => dest.Description, opt => opt.Condition(src => src.Description != null));
        }
    }
}
