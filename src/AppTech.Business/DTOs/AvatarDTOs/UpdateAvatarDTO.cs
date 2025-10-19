using AppTech.Business.DTOs.Commons;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.AvatarDTOs
{
    public class UpdateAvatarDTO : BaseEntityDTO, IAuditedEntityDTO
    {
        public bool Gender { get; set; }
        public IFormFile Image { get; set; }
    }
}
