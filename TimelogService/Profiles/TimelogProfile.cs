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
                .ForMember(dest => dest.TaskId, opt => opt.Condition((src, _, _) => src.TaskId != null))
                .ForMember(dest => dest.HoursSpent, opt => opt.Condition((src, _, _) => src.HoursSpent != null))
                .ForMember(dest => dest.Date, opt => opt.Condition((src, _, _) => src.Date != null));

            CreateMap<Timelog, TimelogConfirmationDTO>()
                .ForMember(dest => dest.Username, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.UserRole, opt => opt.Ignore())
                .ForMember(dest => dest.TaskTitle, opt => opt.Ignore())
                .ForMember(dest => dest.TaskStatus, opt => opt.Ignore());
        }
    }
}
