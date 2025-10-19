using AppTech.Business.DTOs.Commons;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.UserDTOs
{
    public class UpdateUserDTO : IAuditedEntityDTO
    {
        public string username { get; set; }
        public string email { get; set; }
        public string fullName { get; set; }
        public string phoneNumber { get; set; }
        public IFormFile Image { get; set; }
    }
    public class UpdateUserResponseDTO
    {
        public string Message { get; set; }
    }
}
