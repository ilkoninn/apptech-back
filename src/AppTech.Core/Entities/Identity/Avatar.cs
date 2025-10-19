using AppTech.Core.Entities.Commons;

namespace AppTech.Core.Entities.Identity
{
    public class Avatar : BaseEntity, IAuditedEntity
    {
        public string ImageUrl { get; set; }
        public bool Gender { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
