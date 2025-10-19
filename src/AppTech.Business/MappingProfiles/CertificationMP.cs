using AppTech.Business.DTOs.CertificationDTOs;
using AppTech.Business.MappingProfiles.Utilities;
using AppTech.Core.Entities;
using AutoMapper;

namespace AppTech.Business.MappingProfiles
{
    public class CertificationMP : Profile
    {
        public CertificationMP()
        {
            // Create Certification

            CreateMap<CreateCertificationDTO, Certification>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .AfterMap<CustomMappingAction<CreateCertificationDTO, Certification>>()
                .ReverseMap();
            CreateMap<CreateCertificationTranslationDTO, CertificationTranslation>()
                .ReverseMap();

            // Update Certification

            CreateMap<UpdateCertificationDTO, Certification>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .AfterMap<CustomMappingAction<UpdateCertificationDTO, Certification>>()
                .ReverseMap();
            CreateMap<UpdateCertificationTranslationDTO, CertificationTranslation>()
                .ReverseMap();
        }
    }
}
