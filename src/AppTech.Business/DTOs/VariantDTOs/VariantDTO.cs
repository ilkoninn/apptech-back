using AppTech.Business.DTOs.Commons;

namespace AppTech.Business.DTOs.VariantDTOs
{
    public class VariantDTO : BaseEntityDTO
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}
