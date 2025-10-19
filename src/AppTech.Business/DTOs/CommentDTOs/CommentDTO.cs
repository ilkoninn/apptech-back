namespace AppTech.Business.DTOs.CommentDTOs
{
    public class CommentDTO
    {
        public string UserImage { get; set; }
        public string UserName { get; set; }
        public string Subject { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CertificationSlug { get; set; }
    }
}
