using System.Text.Json.Serialization;
using AppTech.Business.DTOs.Commons;
using AppTech.Business.DTOs.VariantDTOs;
using AppTech.Core.Entities;
using AppTech.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.QuestionDTOs
{
    public class UpdateQuestionDTO : BaseEntityDTO
    {
        public int? Server { get; set; }
        public double Point { get; set; }
        public string Content { get; set; }
        public EQuestionType Type { get; set; }
        public int CertificationId { get; set; }
        public ICollection<IFormFile>? Images { get; set; }

        [JsonIgnore]
        public int PageIndex { get; set; }
        public List<UpdateVariantDTO>? Variants { get; set; }
        public ICollection<QuestionImage>? QuestionImages { get; set; }
        public Certification? Certification { get; set; }
    }
}
