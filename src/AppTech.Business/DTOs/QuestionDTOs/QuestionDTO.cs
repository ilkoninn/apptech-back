using System.Text.Json.Serialization;
using AppTech.Business.DTOs.Commons;
using AppTech.Business.DTOs.VariantDTOs;
using AppTech.Core.Entities;
using AppTech.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.QuestionDTOs
{
    public class QuestionDTO : BaseEntityDTO
    {
        public string Content { get; set; }
        public string Type { get; set; }

        // Multiple and Single Questions 
        public ICollection<string>? ImageUrls { get; set; }
        public ICollection<VariantDTO>? Variants { get; set; }
        public IEnumerable<string>? BaseImageUrls { get; set; }
        public int? Server { get; set; }

        // Drag And Drop Questions 
        public DragDropQuestionResponseDTO? DragDropQuestion { get; set; }

        [JsonIgnore]
        public string ReportFrom { get; set; }
        public bool IsReported { get; set; }
        public int PageIndex { get; set; }
        public List<VariantDTO>? DashVariants { get; set; }
        public ICollection<QuestionImage>? QuestionImages { get; set; }
        public Certification? Certification { get; set; }
        public double Point { get; set; }
    }

    public class GetAllQuestionsFromPdfDTO
    {
        public EQuestionType Type { get; set; }
        public string Content { get; set; }
        public string Point { get; set; }
        public string ImageUrl { get; set; }
        public List<VariantDTOForPdf> Variants { get; set; }
    }

    public class VariantDTOForPdf
    {
        public string Key { get; set; }
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}
