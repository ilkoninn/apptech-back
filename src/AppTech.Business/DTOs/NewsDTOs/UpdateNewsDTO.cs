using System.Text.Json.Serialization;
using AppTech.Business.DTOs.Commons;
using AppTech.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.NewsDTOs
{
    public class UpdateNewsDTO : BaseEntityDTO, IAuditedEntityDTO
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public IFormFile Image { get; set; }

        [JsonIgnore]
        public List<UpdateNewsTranslationDTO> Translations { get; set; }
        public string DashImageUrl { get; set; }
        public int PageIndex { get; set; }
    }
    public class UpdateNewsTranslationDTO : BaseEntityDTO
    {
        public int NewsId { get; set; }
        public string Description { get; set; }
        public ELanguage Language { get; set; }
    }
}
