using AutoMapper;
using TimelogService.Models;
using TimelogService.Models.DTO;

namespace TimelogService.Profiles
{
    public class TimelogProfile : Profile
    {
        public TimelogProfile()
        {
            CreateMap<TimelogCreationDTO, Timelog>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.LoggedByUserId, opt => opt.Ignore());

            CreateMap<Timelog, TimelogDTO>();

            CreateMap<TimelogUpdateDTO, Timelog>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.LoggedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.Condition((src, _, _) => src.ProjectId != null))
                .ForMember(dest => dest.WorkPackageId, opt => opt.Condition((src, _, _) => src.WorkPackageId != null))
                .ForMember(dest => dest.HoursSpent, opt => opt.Condition((src, _, _) => src.HoursSpent != null))
                .ForMember(dest => dest.Date, opt => opt.Condition((src, _, _) => src.Date != null));

            CreateMap<Timelog, TimelogConfirmationDTO>()
                .ForMember(dest => dest.Username, opt => opt.Ignore())
                .ForMember(dest => dest.WorkPackageTitle, opt => opt.Ignore());
        }
    }
}
