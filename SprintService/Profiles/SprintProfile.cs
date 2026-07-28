using AutoMapper;
using SprintService.Models;
using SprintService.Models.DTO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SprintService.Profiles
{
    public class SprintProfile : Profile
    {
        public SprintProfile()
        {
            CreateMap<SprintCreationDTO, Sprint>();

            CreateMap<Sprint, SprintDTO>();

            CreateMap<SprintUpdateDTO, Sprint>();

            CreateMap<Sprint, SprintConfirmationDTO>();
        }
    }
}