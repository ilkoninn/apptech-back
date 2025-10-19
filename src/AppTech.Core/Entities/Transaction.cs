using System.ComponentModel.DataAnnotations.Schema;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Entities.Identity;
using AppTech.Core.Enums;

namespace AppTech.Core.Entities
{
    public class Transaction : BaseEntity, IAuditedEntity
    {
        public User User { get; set; }
        public long OrderId { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public EOrderType Type { get; set; }
        public bool IsIncreased { get; set; }
        public string SessionId { get; set; }
        public string CheckToken { get; set; }
        public EOrderStatus Status { get; set; }
        public int? SubscriptionId { get; set; }
        public string? ResponseBody { get; set; }
        public int? CertificationId { get; set; }
        public string? PromotionCode { get; set; }
        public Subscription? Subscription { get; set; }
        public Certification? Certification { get; set; }
        public ICollection<TransactionTranslation> TransactionTranslations { get; set; }   

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class TransactionTranslation : BaseEntity, IAuditedEntity
    {
        public int TransactionId { get; set; }
        public Transaction Transaction { get; set; }
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
