using System.Text.Json.Serialization;
using AppTech.Business.DTOs.Commons;
using AppTech.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.CompanyDTOs
{
    public class UpdateCompanyDTO : BaseEntityDTO, IAuditedEntityDTO
    {
        public string Title { get; set; }
        public IFormFile Image { get; set; }
        public bool IsTop { get; set; }

        [JsonIgnore]
        public List<UpdateCompanyTranslationDTO> Translations { get; set; }
        public string DashImageUrl { get; set; }
        public int PageIndex { get; set; }
    }
    public class UpdateCompanyTranslationDTO : BaseEntityDTO
    {
        public int CompanyId { get; set; }
        public string SecTitle { get; set; }
        public string SecDescription { get; set; }
        public ELanguage Language { get; set; }
    }
}
