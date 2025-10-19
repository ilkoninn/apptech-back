using AppTech.Business.DTOs.ContactUsDTOs;
using AppTech.Business.MappingProfiles.Utilities;
using AppTech.Core.Entities;
using AutoMapper;

namespace AppTech.Business.MappingProfiles
{
    public class ContactUsMP : Profile
    {
        public ContactUsMP()
        {
            // Create ContactUs

            CreateMap<CreateContactUsDTO, ContactUs>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .AfterMap<CustomMappingAction<CreateContactUsDTO, ContactUs>>()
                .ReverseMap();

            // Update ContactUs

            CreateMap<UpdateContactUsDTO, ContactUs>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .AfterMap<CustomMappingAction<UpdateContactUsDTO, ContactUs>>()
                .ReverseMap();
        }
    }
}
