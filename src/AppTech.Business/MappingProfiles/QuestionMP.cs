using AppTech.Business.DTOs.QuestionDTOs;
using AppTech.Core.Entities;
using AutoMapper;

namespace AppTech.Business.MappingProfiles.QuestionProfile
{
    public class QuestionMP : Profile
    {
        public QuestionMP()
        {
            CreateMap<Question, CreateQuestionDTO>().ReverseMap();
            CreateMap<Question, UpdateQuestionDTO>().ReverseMap();
        }
    }
}
