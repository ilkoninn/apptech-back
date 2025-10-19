using System.Text.Json.Serialization;
using System.Transactions;
using AppTech.Business.DTOs.Commons;
using AppTech.Core.Entities;

namespace AppTech.Business.DTOs.ExamDTOs
{
    public class ExamDTO : BaseEntityDTO
    {
        // Get By Slug/Id Section
        public string Slug { get; set; }
        public string CertificationSlug { get; set; }
        public int CertificationId { get; set; }
        public string Code { get; set; }
        public double PassScore { get; set; }
        public double MaxScore { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
        public int QuestionCount { get; set; }
        public string Description { get; set; }
        public int? LastVersion { get; set; }


        // Get All Exam Section
        public string CompanyTitle { get; set; }
        public string CertificationTitle { get; set; }
        public string ImageUrl { get; set; }
        public ICollection<string> Types { get; set; }

        [JsonIgnore]
        public List<ExamTranslationDTO> Translations { get; set; }
    }

    public class ExamTranslationDTO : BaseEntityDTO
    {
        public int ExamId { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
    }
}
