using AppTech.Business.DTOs.CertificationDTOs;
using AppTech.Business.DTOs.ExamDTOs;
using AppTech.Business.MappingProfiles.Utilities;
using AppTech.Core.Entities;
using AutoMapper;

namespace AppTech.Business.MappingProfiles
{
    public class ExamMP : Profile
    {
        public ExamMP()
        {
            // Create Exam

            CreateMap<CreateExamDTO, Exam>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .AfterMap<CustomMappingAction<CreateExamDTO, Exam>>()
                .ReverseMap();
            CreateMap<CreateExamTranslationDTO, ExamTranslation>().ReverseMap();

            // Update Exam

            CreateMap<UpdateExamDTO, Exam>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .AfterMap<CustomMappingAction<UpdateExamDTO, Exam>>()
                .ReverseMap(); ;
            CreateMap<UpdateExamTranslationDTO, ExamTranslation>().ReverseMap();
        }
    }
}
