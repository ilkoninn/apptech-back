
using System.Text.Json.Serialization;
using AppTech.Business.DTOs.Commons;
using AppTech.Core.Enums;

namespace AppTech.Business.DTOs.FAQDTOs
{
    public class UpdateFAQDTO : BaseEntityDTO
    {
        public ELanguage Language { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }

        [JsonIgnore]
        public int PageIndex { get; set; }
    }
}
