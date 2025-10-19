using AppTech.Business.DTOs.Commons;
using AppTech.Core.Entities.Commons;

namespace AppTech.Business.DTOs.CertificationDTOs
{
    public class CertificationUserResponseDTO
    {
        public string ImageUrl { get; set; }
        public string Title { get; set; }
        public string CompanyTitle { get; set; }
        public string Slug { get; set; }
        public int? LastVersion { get; set; }
    }
}
