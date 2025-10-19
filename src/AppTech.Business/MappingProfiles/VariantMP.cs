using AppTech.Business.DTOs.VariantDTOs;
using AppTech.Core.Entities;
using AutoMapper;

namespace AppTech.Business.MappingProfiles
{
    public class VariantMP : Profile
    {
        public VariantMP()
        {
            CreateMap<CreateVariantDTO, Variant>(); // DTO-dan Entity-ə
            CreateMap<Variant, CreateVariantDTO>().ReverseMap(); // Entity-dən DTO-ya
            CreateMap<UpdateVariantDTO, Variant>();
            CreateMap<Variant, UpdateVariantDTO>().ReverseMap();
        }
    }
}
