using AutoMapper;
using NotificationService.Models;
using NotificationService.Models.DTO.NotificationDTOs;

namespace NotificationService.Profiles
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            CreateMap<NotificationCreateDTO, Notification>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Message));

            CreateMap<Notification, NotificationDisplayDTO>();
        }
    }
}
