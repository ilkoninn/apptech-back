using AppTech.Business.DTOs.SettingDTOs;
using AppTech.Core.Entities;
using AutoMapper;

namespace AppTech.Business.MappingProfiles
{
    public class SettingMP : Profile
    {
        public SettingMP()
        {
            // Create Setting

            CreateMap<CreateSettingDTO, Setting>()
                .ReverseMap();
            CreateMap<CreateSettingTranslationDTO, SettingTranslation>()
                .ReverseMap();

            // Update Setting

            CreateMap<UpdateSettingDTO, Setting>()
                .ReverseMap();
            CreateMap<UpdateSettingTranslationDTO, SettingTranslation>()
                .ReverseMap();
        }
    }
}
