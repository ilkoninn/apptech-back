using AppTech.Core.Entities.Commons;
using AppTech.Core.Enums;

namespace AppTech.Core.Entities
{
    public class Setting : BaseEntity, IAuditedEntity
    {
        public string Key { get; set; }
        public string Type { get; set; }
        public string Page { get; set; }
        public List<SettingTranslation> SettingTranslation { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class SettingTranslation : BaseEntity, IAuditedEntity
    {
        public int SettingId { get; set; }
        public Setting Setting { get; set; }
        public string Value { get; set; }
        public ELanguage Language { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
