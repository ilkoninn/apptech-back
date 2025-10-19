using AppTech.Business.DTOs.Commons;

namespace AppTech.Business.DTOs.VariantDTOs
{
    public class UpdateVariantDTO : BaseEntityDTO
    {
        public int QuestionId { get; set; }
        public string? Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}
