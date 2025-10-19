using AppTech.Business.DTOs.Commons;

namespace AppTech.Business.DTOs.CommentDTOs
{
    public class GetByIdCommentDTO : BaseEntityDTO { }
    public class GetBySlugCommentDTO
    {
        public string slug { get; set; }
    }
}
