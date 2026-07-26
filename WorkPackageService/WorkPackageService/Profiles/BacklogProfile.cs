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
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
