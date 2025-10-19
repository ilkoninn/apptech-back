using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.Commons
{
    public interface IAuditedEntityDTO
    {
        public IFormFile Image { get; set; }
    }
}
