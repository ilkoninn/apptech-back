using AppTech.Business.DTOs.Commons;

namespace AppTech.Business.DTOs.CommentDTOs
{
    public class UpdateCommentDTO : BaseEntityDTO
    {
        public string userId { get; set; }
        public string subject { get; set; }
    }
}
