using AppTech.Business.DTOs.PromotionDTOs;
using AppTech.Core.Entities;
using AutoMapper;

namespace AppTech.Business.MappingProfiles
{
    public class PromotionMP : Profile
    {
        public PromotionMP()
        {
            CreateMap<CreatePromotionDTO, Promotion>().ReverseMap();
            CreateMap<UpdatePromotionDTO, Promotion>().ReverseMap();
        }
    }
}
