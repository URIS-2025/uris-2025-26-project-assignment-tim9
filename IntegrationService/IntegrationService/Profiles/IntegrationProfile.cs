using AutoMapper;
using IntegrationService.Models;
using IntegrationService.Models.DTO.IntegrationDTOs;

namespace IntegrationService.Profiles
{
    public class IntegrationProfile : Profile
    {
        public IntegrationProfile()
        {
            // ApiKeyEncrypted se namerno ne mapira ovde - postavlja se eksplicitno u kontroleru
            // nakon enkripcije preko IApiKeyProtector, da AutoMapper profil ne bi zavisio od DI.
            CreateMap<IntegrationCreateDTO, Integration>()
                .ForMember(dest => dest.ApiKeyEncrypted, opt => opt.Ignore());

            CreateMap<IntegrationUpdateDTO, Integration>()
                .ForMember(dest => dest.ApiKeyEncrypted, opt => opt.Ignore());

            // ApiKeyMasked takodje zahteva pristup protektoru (za dekripciju radi maskiranja),
            // pa se postavlja eksplicitno u kontroleru posle mapiranja.
            CreateMap<Integration, IntegrationDisplayDTO>()
                .ForMember(dest => dest.ApiKeyMasked, opt => opt.Ignore());
        }
    }
}
