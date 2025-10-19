using System.ComponentModel.DataAnnotations.Schema;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Enums;

namespace AppTech.Core.Entities
{
    public class Exam : BaseEntity, IAuditedEntity
    {
        public string Code { get; set; }
        public string Slug { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
        public DateTime TakeOn { get; set; }
        public string? ImageUrl { get; set; }
        public double MaxScore { get; set; }
        public double PassScore { get; set; }
        public int QuestionCount { get; set; }
        public int? ExamPackageId { get; set; }
        public int CertificationId { get; set; }
        public ExamPackage? ExamPackage { get; set; }
        public Certification Certification { get; set; }
        public ICollection<ExamTranslation> ExamTranslations { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class ExamTranslation : BaseEntity, IAuditedEntity
    {
        public int ExamId { get; set; }
        public Exam Exam { get; set; }
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
