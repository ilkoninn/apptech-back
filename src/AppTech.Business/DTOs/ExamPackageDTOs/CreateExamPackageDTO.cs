using AppTech.Business.DTOs.Commons;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.ExamPackageDTOs
{
    public class CreateExamPackageDTO : IAuditedEntityDTO
    {
        public string Title { get; set; }
        public decimal Price { get; set; }
        public IFormFile Image { get; set; }
        public ICollection<int> ExamIds { get; set; }
        public string? SpecificDomain { get; set; }
    }
}
