using System.Text.Json.Serialization;
using AppTech.Business.DTOs.Commons;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.ExamPackageDTOs
{
    public class UpdateExamPackageDTO : BaseEntityDTO, IAuditedEntityDTO
    {
        public string Title { get; set; }
        public decimal Price { get; set; }
        public IFormFile Image { get; set; }
        public ICollection<int> ExamIds { get; set; }
        public string? SpecificDomain { get; set; }

        [JsonIgnore]
        public string DashImageUrl { get; set; }
        public int PageIndex { get; set; }
    }
}
