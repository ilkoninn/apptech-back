using AppTech.Business.DTOs.QuestionDTOs;

namespace AppTech.Business.DTOs.ExamDTOs
{
    public class CreateRandomExamDTO
    {
        public int examId { get; set; }
        public bool isReset { get; set; }
        public string userId { get; set; }
        public string? examToken { get; set; }
        public int? examTime { get; set; }
    }

    public class RandomExamDTO
    {
        // Fail to take an exam
        public string? AlertImageUrl { get; set; }
        public string? AlertTitle { get; set; }
        public string? AlertDescription { get; set; }

        // Success to take an exam
        public string? Title { get; set; }
        public int? Duration { get; set; }
        public bool IsTerminal { get; set; }
        public int? ServerCount { get; set; }
        public string? ExamToken { get; set; }
        public int? QuestionCount { get; set; }
        public string? Introduction { get; set; }
        public ICollection<string>? Terminals { get; set; }
        public ICollection<QuestionDTO>? Questions { get; set; }
    }

    public class SubmitExamDTO
    {
        public int? time { get; set; }
        public int examId { get; set; }
        public string userId { get; set; }
        public ICollection<string>? terminals { get; set; }
        public string examToken { get; set; }
        public ICollection<SubmitAnswerDTO>? answers { get; set; }
    }

    public class DropZoneAnswerDTO
    {
        public int dropZoneId { get; set; }
        public int dragVariantId { get; set; }
    }

    public class SubmitAnswerDTO
    {
        public int questionId { get; set; }
        public ICollection<int>? variantIds { get; set; }
        public ICollection<DropZoneAnswerDTO>? dropZoneAnswers { get; set; }
    }
}
