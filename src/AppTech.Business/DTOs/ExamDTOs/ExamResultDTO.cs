namespace AppTech.Business.DTOs.ExamDTOs
{
    public class ExamResultDTO
    {
        public bool IsPassed { get; set; }
        public bool IsTerminal { get; set; }
        public string? Title { get; set; }
        public double MaxScore { get; set; }
        public double UserScore { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? ExamOn { get; set; }
        public int? LastVersion { get; set; }
        public double? PassScore { get; set; }
        public string? Description { get; set; }
    }
}
