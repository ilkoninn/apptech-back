using AppTech.Core.Entities.Commons;

namespace AppTech.Core.Entities
{
    public class Partner : BaseEntity, IAuditedEntity
    {
        // Partner Front Section
        public string? Url { get; set; }
        public string? ImageUrl { get; set; }

        // Partner Back Section
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Position { get; set; }
        public string? Company { get; set; }
        public bool IsAccepted { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
