using System.Text.Json.Serialization;
using AppTech.Business.DTOs.Commons;
using AppTech.Core.Enums;

namespace AppTech.Business.DTOs.SettingDTOs
{
    public class UpdateSettingDTO : BaseEntityDTO
    {
        public string Key { get; set; }
        public string Type { get; set; }
        public string Page { get; set; }

        [JsonIgnore]
        public int PageIndex { get; set; }
        public List<UpdateSettingTranslationDTO> Translations { get; set; }
    }
    public class UpdateSettingTranslationDTO : BaseEntityDTO
    {
        public int SettingId { get; set; }
        public string Value { get; set; }
        public ELanguage Language { get; set; }
    }
}
