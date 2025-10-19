using AppTech.Business.DTOs.Commons;

namespace AppTech.Business.DTOs.PartnerDTOs
{
    public class PartnerDTO : BaseEntityDTO
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
    }
}
