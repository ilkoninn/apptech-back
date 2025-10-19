namespace AppTech.Business.DTOs.ExamDTOs
{
    public class QuestionAnswerDTO
    {
        public int QuestionId { get; set; }
        public List<int> SelectedVariantIds { get; set; }
    }
}
