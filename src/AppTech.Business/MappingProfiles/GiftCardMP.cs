using AppTech.Business.DTOs.GiftCardDTOs;
using AppTech.Business.MappingProfiles.Utilities;
using AppTech.Core.Entities;
using AutoMapper;

namespace AppTech.Business.MappingProfiles
{
    public class GiftCardMP : Profile
    {
        public GiftCardMP()
        {
            // Create GiftCard

            CreateMap<CreateGiftCardDTO, GiftCard>()
                 .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                 .AfterMap<CustomMappingAction<CreateGiftCardDTO, GiftCard>>()
                 .ReverseMap();

            // Update GiftCard

            CreateMap<UpdateGiftCardDTO, GiftCard>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .AfterMap<CustomMappingAction<UpdateGiftCardDTO, GiftCard>>()
                .ReverseMap();
        }
    }
}
