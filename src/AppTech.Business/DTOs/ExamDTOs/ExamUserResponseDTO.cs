namespace AppTech.Business.DTOs.ExamDTOs
{
    public class ExamUserResponseDTO
    {
        public string ImageUrl { get; set; }
        public string Title { get; set; }
        public string CompanyTitle { get; set; }
        public string Slug { get; set; }
        public int? Count { get; set; }
        public bool? IsSub {  get; set; }
        public int? LastVersion { get; set; }
    }
}
