using AppTech.Core.Entities.Commons;

namespace AppTech.Core.Entities
{
    public class CertificationPromotion : BaseEntity, IAuditedEntity
    {
        public int CertificationId { get; set; }
        public Certification Certification { get; set; }
        public int PromotionId { get; set; }
        public Promotion Promotion { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
