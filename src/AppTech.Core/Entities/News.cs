using AppTech.Core.Entities.Commons;
using AppTech.Core.Enums;

namespace AppTech.Core.Entities
{
    public class News : BaseEntity, IAuditedEntity
    {
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }
        public ICollection<NewsTranslation> NewsTranslations { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
    public class NewsTranslation : BaseEntity, IAuditedEntity
    {
        public int NewsId { get; set; }
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
