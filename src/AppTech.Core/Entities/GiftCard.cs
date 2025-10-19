using System.ComponentModel.DataAnnotations.Schema;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Enums;

namespace AppTech.Core.Entities
{
    public class GiftCard : BaseEntity, IAuditedEntity
    {
        public string ImageUrl { get; set; }
        public EGiftCardType Type { get; set; }
        public decimal Price { get; set; }
        public string? Message { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
