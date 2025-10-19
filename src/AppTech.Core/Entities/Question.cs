using AppTech.Core.Entities.Commons;
using AppTech.Core.Enums;

namespace AppTech.Core.Entities
{
    public class Question : BaseEntity, IAuditedEntity
    {
        public int CertificationId { get; set; }
        public Certification Certification { get; set; }
        public double Point { get; set; }
        public string Content { get; set; }
        public int? Server { get; set; } // Red Hat Exams
        public bool IsReported { get; set; }
        public string? ReportFrom { get; set; }
        public EQuestionType Type { get; set; }
        public ICollection<Variant>? Variants { get; set; }
        public ICollection<QuestionImage>? QuestionImages { get; set; }
        public DragDropQuestion? DragDropQuestion { get; set; }

        // base fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class QuestionImage : BaseEntity, IAuditedEntity
    {
        public int QuestionId { get; set; }
        public Question Question { get; set; }
        public string ImageUrl { get; set; }
        // base fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class DragDropQuestion : BaseEntity, IAuditedEntity
    {
        public int QuestionId { get; set; }
        public Question Question { get; set; }
        public string ImageUrl { get; set; }
        public ICollection<DropZoneDragVariant> DropZoneDragVariants { get; set; }

        // base fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class DropZoneDragVariant : BaseEntity, IAuditedEntity
    {
        public int DragDropQuestionId { get; set; }
        public DragDropQuestion DragDropQuestion { get; set; }
        public int DragVariantId { get; set; }
        public DragVariant DragVariant { get; set; }
        public int DropZoneId { get; set; }
        public DropZone DropZone { get; set; }

        // base fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class DropZone : BaseEntity, IAuditedEntity
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public ICollection<DropZoneDragVariant> DropZoneDragVariants { get; set; }

        // base fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class DragVariant : BaseEntity, IAuditedEntity
    {
        public string ImageUrl { get; set; }
        public ICollection<DropZoneDragVariant> DropZoneDragVariants { get; set; }

        // base fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
