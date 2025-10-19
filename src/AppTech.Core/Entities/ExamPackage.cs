using System.ComponentModel.DataAnnotations.Schema;
using AppTech.Core.Entities.Commons;

namespace AppTech.Core.Entities
{
    public class ExamPackage : BaseEntity, IAuditedEntity
    {
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public ICollection<Exam> Exams { get; set; }
        public string? SpecificDomain { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
