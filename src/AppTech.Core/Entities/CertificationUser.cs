using AppTech.Core.Entities.Commons;
using AppTech.Core.Entities.Identity;

namespace AppTech.Core.Entities
{
    public class CertificationUser : BaseEntity, IAuditedEntity
    {
        public int CertificationId { get; set; }
        public Certification Certification { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
