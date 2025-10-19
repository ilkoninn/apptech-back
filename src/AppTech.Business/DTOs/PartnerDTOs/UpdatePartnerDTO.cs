using System.Text.Json.Serialization;
using AppTech.Business.DTOs.Commons;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.PartnerDTOs
{
    public class UpdatePartnerDTO : BaseEntityDTO, IAuditedEntityDTO
    {
        public string Url { get; set; }
        public string Company { get; set; }
        public IFormFile Image { get; set; }

        [JsonIgnore]
        public int PageIndex { get; set; }
        public string DashImageUrl { get; set; }
    }
}
