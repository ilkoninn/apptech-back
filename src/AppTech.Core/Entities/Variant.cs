using AppTech.Core.Entities.Commons;

namespace AppTech.Core.Entities
{
    public class Variant : BaseEntity, IAuditedEntity
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
        //public Question Question { get; set; }
        public int QuestionId { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
