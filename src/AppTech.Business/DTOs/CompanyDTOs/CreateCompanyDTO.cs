using System.Text.Json.Serialization;
using AppTech.Business.DTOs.Commons;
using AppTech.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.CompanyDTOs
{
    public class CreateCompanyDTO : IAuditedEntityDTO
    {
        public string Title { get; set; }
        public IFormFile Image { get; set; }
        public bool IsTop { get; set; }

        [JsonIgnore]
        public List<CreateCompanyTranslationDTO> Translations { get; set; }
    }
    public class CreateCompanyTranslationDTO
    {
        public int CompanyId { get; set; }
        public string SecTitle { get; set; }
        public string SecDescription { get; set; }
        public ELanguage Language { get; set; }
    }
}
