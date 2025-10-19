using AppTech.Core.Entities.Commons;

namespace AppTech.Core.Entities
{
    public class ContactUs : BaseEntity, IAuditedEntity
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? ImageUrl { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
