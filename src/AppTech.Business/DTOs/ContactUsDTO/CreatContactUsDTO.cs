using AppTech.Business.DTOs.Commons;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.ContactUsDTOs
{
    public class CreateContactUsDTO : IAuditedEntityDTO
    {
        public string fullName { get; set; }
        public string email { get; set; }
        public string message { get; set; }
        public string subject { get; set; }
        public IFormFile? Image { get; set; }
    }
}
