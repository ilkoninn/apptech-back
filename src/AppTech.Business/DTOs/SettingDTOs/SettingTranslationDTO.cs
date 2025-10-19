using AppTech.Business.DTOs.Commons;

namespace AppTech.Business.DTOs.SettingDTOs
{
    public class SettingTranslationDTO : BaseEntityDTO
    {
        public int SettingId { get; set; }
        public string Value { get; set; }
        public string Language { get; set; }
    }
}
