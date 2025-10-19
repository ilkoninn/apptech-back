using System.Text.Json.Serialization;

namespace AppTech.Business.DTOs.SettingDTOs
{
    public class SettingDTO
    {
        public int? Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Page { get; set; }    

        [JsonIgnore]
        public string Type { get; set; }
        public List<SettingTranslationDTO> Translations { get; set; }
    }
}
