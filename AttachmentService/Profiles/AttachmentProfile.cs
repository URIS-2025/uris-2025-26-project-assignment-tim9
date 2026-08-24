using AutoMapper;
using AttachmentService.Models;
using AttachmentService.Models.DTO;

namespace AttachmentService.Profiles
{
    public class AttachmentProfile : Profile
    {
        public AttachmentProfile()
        {
            // Everything else on Attachment (Id, FileName, StoragePath, Status, ...) is set
            // explicitly by the repository after this mapping, not derived from the DTO -
            // declared here so AssertConfigurationIsValid() documents that as intentional
            // instead of silently passing by omission.
            CreateMap<AttachmentCreationDTO, Attachment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FileName, opt => opt.Ignore())
                .ForMember(dest => dest.StoragePath, opt => opt.Ignore())
                .ForMember(dest => dest.Checksum, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UploadedByUserId, opt => opt.Ignore());

            CreateMap<Attachment, AttachmentDTO>();

            // Rename/describe only - skips null source fields so a partial update can't wipe
            // out fields the client didn't send (see AttachmentRepository.UpdateAttachment).
            var updateMap = CreateMap<AttachmentUpdateDTO, Attachment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OriginalFileName, opt => opt.Ignore())
                .ForMember(dest => dest.StoragePath, opt => opt.Ignore())
                .ForMember(dest => dest.ContentType, opt => opt.Ignore())
                .ForMember(dest => dest.FileSize, opt => opt.Ignore())
                .ForMember(dest => dest.Checksum, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.TaskId, opt => opt.Ignore())
                .ForMember(dest => dest.UploadedByUserId, opt => opt.Ignore());

            updateMap.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
