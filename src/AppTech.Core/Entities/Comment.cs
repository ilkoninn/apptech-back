using System.ComponentModel.DataAnnotations.Schema;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Entities.Identity;

namespace AppTech.Core.Entities
{
    public class Comment : BaseEntity, IAuditedEntity
    {
        public string UserId { get; set; }
        public User User { get; set; }

        // Foreign Key to Certification by Slug
        public string CertificationSlug { get; set; }

        [ForeignKey(nameof(CertificationSlug))]
        public Certification Certification { get; set; }

        public string Subject { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }

}
