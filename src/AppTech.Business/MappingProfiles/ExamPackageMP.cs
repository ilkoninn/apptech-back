using AppTech.Business.DTOs.ExamPackageDTOs;
using AppTech.Business.MappingProfiles.Utilities;
using AppTech.Core.Entities;
using AutoMapper;

namespace AppTech.Business.MappingProfiles
{
    public class ExamPackageMP : Profile
    {
        public ExamPackageMP()
        {
            // Create ExamPackage

            CreateMap<CreateExamPackageDTO, ExamPackage>()
                 .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                 .AfterMap<CustomMappingAction<CreateExamPackageDTO, ExamPackage>>()
                 .ReverseMap();

            // Update ExamPackage

            CreateMap<UpdateExamPackageDTO, ExamPackage>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .AfterMap<CustomMappingAction<UpdateExamPackageDTO, ExamPackage>>()
                .ReverseMap();
        }
    }
}
