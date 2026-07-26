using AutoMapper;
using WorkPackageService.Models;
using WorkPackageService.Models.DTO.WorkPackageDTOs;

namespace WorkPackageService.Profiles
{
    public class WorkPackageProfile : Profile
    {
        public WorkPackageProfile()
        {
            CreateMap<WorkPackage, WorkPackageDisplayDTO>();

            CreateMap<WorkPackageCreateDTO, WorkPackage>();

            CreateMap<WorkPackageUpdateDTO, WorkPackage>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
