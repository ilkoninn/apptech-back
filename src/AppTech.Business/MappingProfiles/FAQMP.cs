using AppTech.Business.DTOs.FAQDTOs;
using AppTech.Core.Entities;
using AutoMapper;

namespace AppTech.Business.MappingProfiles
{
    public class FAQMP : Profile
    {
        public FAQMP()
        {
            CreateMap<CreateFAQDTO, FAQ>().ReverseMap();
            CreateMap<UpdateFAQDTO, FAQ>().ReverseMap();
        }
    }
}
