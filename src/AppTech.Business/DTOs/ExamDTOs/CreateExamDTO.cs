using System.Text.Json.Serialization;
using AppTech.Business.DTOs.Commons;
using AppTech.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.ExamDTOs
{
    public class CreateExamDTO : IAuditedEntityDTO
    {
        public string Code { get; set; }
        public int Duration { get; set; }
        public int MaxScore { get; set; }
        public int PassScore { get; set; }
        public decimal Price { get; set; }
        public IFormFile Image { get; set; }
        public int QuestionCount { get; set; }
        public int CertificationId { get; set; }

        [JsonIgnore]
        public List<CreateExamTranslationDTO> Translations { get; set; }
    }

    public class CreateExamTranslationDTO
    {
        public int ExamId { get; set; }
        public string Description { get; set; }
        public ELanguage Language { get; set; }
    }
}
