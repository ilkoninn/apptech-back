namespace AppTech.Business.DTOs.VariantDTOs
{
    public class CreateVariantDTO
    {
        public int QuestionId { get; set; }
        public string? Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}
