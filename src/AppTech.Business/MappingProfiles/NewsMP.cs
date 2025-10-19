using AppTech.Business.DTOs.NewsDTOs;
using AppTech.Business.MappingProfiles.Utilities;
using AppTech.Core.Entities;
using AutoMapper;

namespace AppTech.Business.MappingProfiles
{
    public class NewsMP : Profile
    {
        public NewsMP()
        {
            // Create News

            CreateMap<CreateNewsDTO, News>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .AfterMap<CustomMappingAction<CreateNewsDTO, News>>()
                .ReverseMap();
            CreateMap<CreateNewsTranslationDTO, NewsTranslation>()
                .ReverseMap();

            // Update News

            CreateMap<UpdateNewsDTO, News>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .AfterMap<CustomMappingAction<UpdateNewsDTO, News>>()
                .ReverseMap();
            CreateMap<UpdateNewsTranslationDTO, NewsTranslation>()
                .ReverseMap();
        }
    }
}
