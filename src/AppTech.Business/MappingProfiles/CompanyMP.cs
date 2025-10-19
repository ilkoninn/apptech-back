using AppTech.Business.DTOs.CompanyDTOs;
using AppTech.Business.MappingProfiles.Utilities;
using AppTech.Core.Entities;
using AutoMapper;

namespace AppTech.Business.MappingProfiles.CompanyProfile
{
    public class CompanyMP : Profile
    {
        public CompanyMP()
        {
            // Create Company

            CreateMap<CreateCompanyDTO, Company>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .AfterMap<CustomMappingAction<CreateCompanyDTO, Company>>()
                .ReverseMap();
            CreateMap<CreateCompanyTranslationDTO, CompanyTranslation>()
                .ReverseMap();

            // Update Company

            CreateMap<UpdateCompanyDTO, Company>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .AfterMap<CustomMappingAction<UpdateCompanyDTO, Company>>()
                .ReverseMap();
            CreateMap<UpdateCompanyTranslationDTO, CompanyTranslation>()
                .ReverseMap();
        }
    }
}
