using AppTech.Business.DTOs.CommentDTOs;

namespace AppTech.Business.Services.Interfaces
{
    public interface ICommentService
    {
        Task<IQueryable<CommentDTO>> GetAllAsync();
        Task<IQueryable<CommentDTO>> GetBySlugAsync(GetBySlugCommentDTO dto);
        Task<CommentDTO> GetByIdAsync(GetByIdCommentDTO dto);
        Task<CommentDTO> AddAsync(CreateCommentDTO dto);
        Task<CommentDTO> UpdateAsync(UpdateCommentDTO dto);
        Task<CommentDTO> DeleteAsync(DeleteCommentDTO dto);
    }
}
