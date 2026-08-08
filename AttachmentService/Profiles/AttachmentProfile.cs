using AutoMapper;
using AttachmentService.Models;
using AttachmentService.Models.DTO;

namespace AttachmentService.Profiles
{
    public class AttachmentProfile : Profile
    {
        public AttachmentProfile()
        {
            CreateMap<AttachmentCreationDTO, Attachment>();

            CreateMap<Attachment, AttachmentDTO>();

            CreateMap<AttachmentUpdateDTO, Attachment>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
