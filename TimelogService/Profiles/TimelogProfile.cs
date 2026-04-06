using AutoMapper;
using TimelogService.Models;
using TimelogService.Models.DTO;

namespace TimelogService.Profiles
{
    public class TimelogProfile : Profile
    {
        public TimelogProfile()
        {
            CreateMap<TimelogCreationDTO, Timelog>();

            CreateMap<Timelog, TimelogDTO>();

            CreateMap<TimelogUpdateDTO, Timelog>();

            CreateMap<Timelog, TimelogConfirmationDTO>();
        }
    }
}