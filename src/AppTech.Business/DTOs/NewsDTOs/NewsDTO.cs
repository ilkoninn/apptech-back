using System.Text.Json.Serialization;
using AppTech.Business.DTOs.Commons;

namespace AppTech.Business.DTOs.NewsDTOs
{
    public class NewsDTO : BaseEntityDTO
    {
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }

        [JsonIgnore]
        public List<NewsTranslationDTO> Translations { get; set; }
    }

    public class NewsTranslationDTO : BaseEntityDTO
    {
        public int NewsId { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
    }
}
