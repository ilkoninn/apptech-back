using System.Text.Json.Serialization;
using AppTech.Business.DTOs.Commons;
using AppTech.Business.DTOs.PromotionDTOs;

namespace AppTech.Business.DTOs.CertificationDTOs
{
    public class CertificationDTO : BaseEntityDTO
    {
        public string Slug { get; set; }
        public int LastVersion { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Code { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountPrice { get; set; }
        public bool IsTrend { get; set; }
        public int CompanyId { get; set; }
        public string CompanySlug { get; set; }
        public int? PromotionId { get; set; }
        public DateTime LastUpdate { get; set; }
        public int QuestionCount { get; set; }
        public bool IsBought { get; set; }
        public SubscriptionResponseDTO? Subcription { get; set; }
        public IEnumerable<PromotionDTO> Promotions { get; set; }

        [JsonIgnore]
        public List<CertificationTranslationDTO> Translations { get; set; }
    }

    public class SubscriptionResponseDTO : BaseEntityDTO
    {
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
    }

    public class CertificationTranslationDTO : BaseEntityDTO
    {
        public int CertificationId { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
    }
}
