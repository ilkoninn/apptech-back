using AppTech.Business.DTOs.PartnerDTOs;
using AppTech.Business.MappingProfiles.Utilities;
using AppTech.Core.Entities;
using AutoMapper;

namespace AppTech.Business.MappingProfiles
{
    public class PartnerMP : Profile
    {
        public PartnerMP()
        {
            // Create Partner

            CreateMap<CreatePartnerDTO, Partner>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .AfterMap<CustomMappingAction<CreatePartnerDTO, Partner>>()
                .ReverseMap();

            CreateMap<CreatePartnerOnlineDTO, Partner>().ReverseMap();

            // Update Partner

            CreateMap<UpdatePartnerDTO, Partner>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .AfterMap<CustomMappingAction<UpdatePartnerDTO, Partner>>()
                .ReverseMap();
        }
    }
}
