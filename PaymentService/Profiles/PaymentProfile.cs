using AutoMapper;
using PaymentService.Models;
using PaymentService.Models.DTO.PaymentDTOs;

namespace PaymentService.Profiles
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            //kreiranje - id, status, datum i platioca postavlja repozitorijum
            CreateMap<PaymentCreationDTO, Payment>()
                .ForMember(dest => dest.PaymentId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Date, opt => opt.Ignore())
                .ForMember(dest => dest.PaidByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore());

            CreateMap<Payment, PaymentDTO>();

            //izmena - Condition preskace polja koja nisu poslata u zahtevu
            CreateMap<PaymentUpdateDTO, Payment>()
                .ForMember(dest => dest.PaymentId, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.Date, opt => opt.Ignore())
                .ForMember(dest => dest.PaidByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.Amount, opt => opt.Condition((src, _, _) => src.Amount != null))
                .ForMember(dest => dest.Status, opt => opt.Condition((src, _, _) => src.Status != null));

            //ime platioca i naziv projekta popunjava repozitorijum iz drugih servisa
            CreateMap<Payment, PaymentConfirmationDTO>()
                .ForMember(dest => dest.PaidByUsername, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectName, opt => opt.Ignore());
        }
    }
}
