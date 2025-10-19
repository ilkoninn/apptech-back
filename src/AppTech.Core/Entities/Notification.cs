using AppTech.Core.Entities.Commons;
using AppTech.Core.Entities.Identity;

namespace AppTech.Core.Entities
{
    public class Notification : BaseEntity, IAuditedEntity
    {
        public bool IsSeen { get; set; }
        public User User { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}

// Optional notification type can be added!
