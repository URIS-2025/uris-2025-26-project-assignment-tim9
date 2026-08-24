using AutoMapper;
using SprintService.Models;
using SprintService.Models.DTO;

namespace SprintService.Profiles
{
    public class SprintProfile : Profile
    {
        public SprintProfile()
        {
            CreateMap<SprintCreationDTO, Sprint>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore());

            CreateMap<Sprint, SprintDTO>();

            CreateMap<SprintUpdateDTO, Sprint>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<Sprint, SprintConfirmationDTO>()
                .ForMember(dest => dest.MilestoneId, opt => opt.Ignore())
                .ForMember(dest => dest.ExpectedDate, opt => opt.Ignore());
        }
    }
}
