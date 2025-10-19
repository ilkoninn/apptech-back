using System.Text.Json.Serialization;
using AppTech.Business.DTOs.Commons;
using AppTech.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.ExamDTOs
{
    public class UpdateExamDTO : BaseEntityDTO, IAuditedEntityDTO
    {
        public string Code { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
        public IFormFile Image { get; set; }
        public double MaxScore { get; set; }
        public double PassScore { get; set; }
        public int QuestionCount { get; set; }
        public int? CertificationId { get; set; }

        [JsonIgnore]
        public List<UpdateExamTranslationDTO> Translations { get; set; }
        public int PageIndex { get; set; }
    }

    public class UpdateExamTranslationDTO : BaseEntityDTO
    {
        public int ExamId { get; set; }
        public string Description { get; set; }
        public ELanguage Language { get; set; }
    }
}
