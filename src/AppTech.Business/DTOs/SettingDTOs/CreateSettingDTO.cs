using AppTech.Core.Enums;

namespace AppTech.Business.DTOs.SettingDTOs
{
    public class CreateSettingDTO
    {
        public string Key { get; set; }
        public string Type { get; set; }
        public string Page { get; set; }
        public List<CreateSettingTranslationDTO> Translations { get; set; }
    }
    public class CreateSettingTranslationDTO
    {
        public int SettingId { get; set; }
        public string Value { get; set; }
        public ELanguage Language { get; set; }
    }
}
