using AppTech.Business.DTOs.Commons;

namespace AppTech.Business.DTOs.AvatarDTOs
{
    public class AvatarDTO : BaseEntityDTO
    {
        public string ImageUrl { get; set; }
        public bool Gender { get; set; }
    }
}
