using System.Text.Json.Serialization;
using AppTech.Business.DTOs.Commons;
using AppTech.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.CertificationDTOs
{
    public class CreateCertificationDTO : IAuditedEntityDTO
    {
        public string Code { get; set; }
        public int LastVersion { get; set; }
        public bool IsTrend { get; set; }
        public decimal Price { get; set; }
        public int CompanyId { get; set; }
        public string SubTitle { get; set; }
        public IFormFile Image { get; set; }
        public decimal DiscountPrice { get; set; }

        [JsonIgnore]
        public List<CreateCertificationTranslationDTO> Translations { get; set; }
    }

    public class CreateCertificationTranslationDTO
    {
        public int CertificationId { get; set; }
        public string Description { get; set; }
        public ELanguage Language { get; set; }
    }
}
