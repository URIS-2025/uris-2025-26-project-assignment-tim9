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
                .ForMember(dest => dest.Name, opt => opt.Condition(src => src.Name != null))
                .ForMember(dest => dest.Description, opt => opt.Condition(src => src.Description != null))
                .ForMember(dest => dest.Status, opt => opt.Condition(src => src.Status != null))
                .ForMember(dest => dest.Deadline, opt => opt.Condition(src => src.Deadline != null)); ;
        }
    }
}
