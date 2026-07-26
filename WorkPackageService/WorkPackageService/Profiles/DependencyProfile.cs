using AutoMapper;
using WorkPackageService.Models;
using WorkPackageService.Models.DTO.DependencyDTOs;

namespace WorkPackageService.Profiles
{
    public class DependencyProfile : Profile
    {
        public DependencyProfile()
        {
            CreateMap<Dependency, DependencyDisplayDTO>();

            CreateMap<DependencyCreateDTO, Dependency>();

            CreateMap<DependencyUpdateDTO, Dependency>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
