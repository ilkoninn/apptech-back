namespace AppTech.Business.DTOs.CommentDTOs
{
    public class CreateCommentDTO
    {
        public string userId { get; set; }
        public string certificationSlug { get; set; }
        public string subject { get; set; }
    }
}
