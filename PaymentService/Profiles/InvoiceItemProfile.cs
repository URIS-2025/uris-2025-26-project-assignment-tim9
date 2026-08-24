using AutoMapper;
using PaymentService.Models;
using PaymentService.Models.DTO.InvoiceItemDTOs;

namespace PaymentService.Profiles
{
    public class InvoiceItemProfile : Profile
    {
        public InvoiceItemProfile()
        {
            //iznos stavke racuna repozitorijum kao UnitPrice * Quantity
            CreateMap<InvoiceItemCreationDTO, InvoiceItem>()
                .ForMember(dest => dest.InvoiceItemId, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore());

            CreateMap<InvoiceItem, InvoiceItemDTO>();

            CreateMap<InvoiceItemUpdateDTO, InvoiceItem>()
                .ForMember(dest => dest.InvoiceItemId, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.Condition((src, _, _) => src.Description != null))
                .ForMember(dest => dest.UnitPrice, opt => opt.Condition((src, _, _) => src.UnitPrice != null))
                .ForMember(dest => dest.Quantity, opt => opt.Condition((src, _, _) => src.Quantity != null));

            //ukupan iznos fakture popunjava repozitorijum nakon preracunavanja
            CreateMap<InvoiceItem, InvoiceItemConfirmationDTO>()
                .ForMember(dest => dest.InvoiceTotalAmount, opt => opt.Ignore());
        }
    }
}
