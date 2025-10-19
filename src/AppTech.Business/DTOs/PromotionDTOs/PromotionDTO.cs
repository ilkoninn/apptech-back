using AppTech.Business.DTOs.Commons;

namespace AppTech.Business.DTOs.PromotionDTOs
{
    public class PromotionDTO : BaseEntityDTO
    {
        public string Code { get; set; }
        public decimal Percentage { get; set; }
    }
}
