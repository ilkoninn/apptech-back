using System.Text.Json.Serialization;
using AppTech.Business.DTOs.Commons;
using AppTech.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.CertificationDTOs
{
    public class UpdateCertificationDTO : BaseEntityDTO, IAuditedEntityDTO
    {
        public string Code { get; set; }
        public int LastVersion { get; set; }
        public bool IsTrend { get; set; }
        public int CompanyId { get; set; }
        public decimal Price { get; set; }
        public string SubTitle { get; set; }
        public IFormFile Image { get; set; }
        public decimal DiscountPrice { get; set; }

        [JsonIgnore]
        public List<UpdateCertificationTranslationDTO> Translations { get; set; }
        public string DashImageUrl { get; set; }
        public int PageIndex { get; set; }
    }

    public class UpdateCertificationTranslationDTO : BaseEntityDTO
    {
        public int CertificationId { get; set; }
        public string Description { get; set; }
        public ELanguage Language { get; set; }
    }
}
