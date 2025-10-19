using AppTech.Business.DTOs.VariantDTOs;
using AppTech.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.QuestionDTOs
{
    public class CreateQuestionDTO
    {
        public int? Server { get; set; }
        public double Point { get; set; }
        public string Content { get; set; }
        public EQuestionType Type { get; set; }
        public int CertificationId { get; set; }
        public ICollection<IFormFile>? Images { get; set; }
        public ICollection<CreateVariantDTO>? Variants { get; set; }
    }
}
