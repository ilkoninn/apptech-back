using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Entities.Identity;
using AppTech.Core.Enums;

namespace AppTech.Core.Entities
{
    public class Certification : BaseEntity, IAuditedEntity
    {
        public string Code { get; set; }
        public string Slug { get; set; }
        public string Title { get; set; }
        public bool IsTrend { get; set; }
        public decimal Price { get; set; }
        public int CompanyId { get; set; }
        public string SubTitle { get; set; }
        public string ImageUrl { get; set; }
        public Company? Company { get; set; }
        public int? LastVersion { get; set; }
        public decimal DiscountPrice { get; set; }
        public ICollection<Exam> Exams { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Question>? Questions { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; }
        public ICollection<CertificationTranslation>? CertificationTranslations { get; set; }

        [JsonIgnore]
        public ICollection<CertificationPromotion>? CertificationPromotions { get; set; }
        public ICollection<CertificationUser> CertificationUsers { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CertificationTranslation : BaseEntity, IAuditedEntity
    {
        public int CertificationId { get; set; }
        public string Description { get; set; }
        public ELanguage Language { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
