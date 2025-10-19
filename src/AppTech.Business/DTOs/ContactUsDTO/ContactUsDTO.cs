using System.Text.Json.Serialization;
using AppTech.Business.DTOs.Commons;

namespace AppTech.Business.DTOs.ContactUsDTOs
{
    public class ContactUsDTO : BaseEntityDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public string? ImageUrl { get; set; }
        public string Subject { get; set; }

        [JsonIgnore]
        public int PageIndex { get; set; }
    }
}
