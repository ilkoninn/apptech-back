using System.Text.Json.Serialization;
using AppTech.Business.DTOs.CertificationDTOs;
using AppTech.Business.DTOs.Commons;

namespace AppTech.Business.DTOs.CompanyDTOs
{
    public class CompanyDTO : BaseEntityDTO
    {
        public string Title { get; set; }
        public string Slug { get; set; }
        public string ImageUrl { get; set; }
        public string? SecTitle { get; set; }
        public string? SecDescription { get; set; }
        public bool? IsTop { get; set; }
        public IEnumerable<CertificationDTO>? CertificationDTOs { get; set; }

        [JsonIgnore]
        public List<CompanyTranslationDTO> Translations { get; set; }
    }

    public class CompanyTranslationDTO : BaseEntityDTO
    {
        public int CompanyId { get; set; }
        public string SecTitle { get; set; }
        public string SecDescription { get; set; }
        public string Language { get; set; }
    }
}
