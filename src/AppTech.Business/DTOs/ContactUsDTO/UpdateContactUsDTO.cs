using AppTech.Business.DTOs.Commons;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.ContactUsDTOs
{
    public class UpdateContactUsDTO : BaseEntityDTO, IAuditedEntityDTO
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? AppUserId { get; set; }
        public string Message { get; set; }
        public IFormFile? Image { get; set; }
    }
}
