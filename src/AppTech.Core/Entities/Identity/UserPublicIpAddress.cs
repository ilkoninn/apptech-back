using AppTech.Core.Entities.Commons;

namespace AppTech.Core.Entities.Identity
{
    public class UserPublicIpAddress : BaseEntity, IAuditedEntity
    {
        public string UserId { get; set; }
        public User User { get; set; }
        public string PublicIpAddress { get; set; }
        public DateTime ExpiredOn { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
