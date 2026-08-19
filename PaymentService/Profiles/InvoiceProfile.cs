using AutoMapper;
using PaymentService.Models;
using PaymentService.Models.DTO.InvoiceDTOs;

namespace PaymentService.Profiles
{
    public class InvoiceProfile : Profile
    {
        public InvoiceProfile()
        {
            //stavke se mapiraju preko InvoiceItemProfile, ukupan iznos racuna repozitorijum
            CreateMap<InvoiceCreationDTO, Invoice>()
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.IssuedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Payments, opt => opt.Ignore());

            CreateMap<Invoice, InvoiceDTO>();

            CreateMap<InvoiceUpdateDTO, Invoice>()
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.IssuedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore())
                .ForMember(dest => dest.Payments, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.Condition((src, _, _) => src.ProjectId != null))
                .ForMember(dest => dest.IssueDate, opt => opt.Condition((src, _, _) => src.IssueDate != null))
                .ForMember(dest => dest.Status, opt => opt.Condition((src, _, _) => src.Status != null));

            CreateMap<Invoice, InvoiceConfirmationDTO>()
                .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count))
                .ForMember(dest => dest.ProjectName, opt => opt.Ignore())
                .ForMember(dest => dest.IssuedByUsername, opt => opt.Ignore());
        }
    }
}
