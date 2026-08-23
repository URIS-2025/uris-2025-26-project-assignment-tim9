using AutoMapper;
using WorkPackageService.Models;
using WorkPackageService.Models.DTO.CommentDTOs;

namespace WorkPackageService.Profiles
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<Comment, CommentDisplayDTO>();

            CreateMap<CommentCreateDTO, Comment>();

            CreateMap<CommentUpdateDTO, Comment>()
                .ForMember(dest => dest.Text, opt => opt.Condition(src => src.Text != null));
        }
    }
}
