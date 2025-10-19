using System.Text.Json.Serialization;
using AppTech.Core.Entities.Commons;
namespace AppTech.Core.Entities
{
    public class Promotion : BaseEntity, IAuditedEntity
    {
        public string Code { get; set; }
        public int Percentage { get; set; }
        public DateTime EndedOn { get; set; }
        [JsonIgnore]
        public ICollection<CertificationPromotion> CertificationPromotions { get; set; }
        //public ICollection<PromotionUser> PromotionUsers { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }

    }
}
