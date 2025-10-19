using AppTech.Business.DTOs.Commons;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.PartnerDTOs
{
    public class CreatePartnerDTO : IAuditedEntityDTO
    {
        public string Url { get; set; }
        public string Company { get; set; }
        public IFormFile Image { get; set; }
    }
}
