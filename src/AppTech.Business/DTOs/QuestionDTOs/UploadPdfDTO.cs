using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.QuestionDTOs
{
    public class UploadPdfDTO
    {
        public int CertificationId { get; set; }
        public IFormFile Pdf { get; set; }
        public string? Questions { get; set; }
    }
}
