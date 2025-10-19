using AppTech.Business.DTOs.AvatarDTOs;
using AppTech.Business.MappingProfiles.Utilities;
using AppTech.Core.Entities.Identity;
using AutoMapper;

namespace AppTech.Business.MappingProfiles
{
    public class AvatarMP : Profile
    {
        public AvatarMP()
        {
            // Create Avatar

            CreateMap<CreateAvatarDTO, Avatar>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .AfterMap<CustomMappingAction<CreateAvatarDTO, Avatar>>()
                .ReverseMap();

            // Update Avatar

            CreateMap<UpdateAvatarDTO, Avatar>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .AfterMap<CustomMappingAction<UpdateAvatarDTO, Avatar>>()
                .ReverseMap();
        }
    }
}
