using AppTech.Core.Entities.Commons;
using AppTech.Core.Enums;

namespace AppTech.Core.Entities
{
    public class Company : BaseEntity, IAuditedEntity
    {
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public bool IsTop { get; set; }
        public string Slug { get; set; }
        public ICollection<Certification> Certifications { get; set; }
        public ICollection<CompanyTranslation> CompanyTranslations { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CompanyTranslation : BaseEntity, IAuditedEntity
    {
        public int CompanyId { get; set; }
        public string SecTitle { get; set; }
        public string SecDescription { get; set; }
        public ELanguage Language { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }

}
