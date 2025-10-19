using AppTech.Business.DTOs.Commons;

namespace AppTech.Business.DTOs.GiftCardDTOs
{
    public class GiftCardDTO : BaseEntityDTO
    {
        public string Type { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
    }
}
