using AppTech.Business.DTOs.UserDTOs;
using AppTech.Business.MappingProfiles.Utilities;
using AppTech.Core.Entities.Identity;
using AutoMapper;

namespace AppTech.Business.MappingProfiles
{
    public class UserMP : Profile
    {
        public UserMP()
        {
            CreateMap<RegisterUserDTO, User>().ReverseMap();
            CreateMap<UpdateUserDTO, User>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .AfterMap<CustomMappingAction<UpdateUserDTO, User>>()
                .ReverseMap();
        }
    }
}
