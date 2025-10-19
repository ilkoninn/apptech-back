using AppTech.Business.DTOs.QuestionDTOs;

namespace AppTech.Business.Services.Interfaces
{
    public interface IQuestionService
    {
        Task<IQueryable<QuestionDTO>> GetAllQuestionWithVariantsAsync(GetAllBySlugDTO dto);
        Task ReportTheSelectedQuestionAsync(ReportQuestionDTO dto);

        public Task<IQueryable<QuestionDTO>> GetAllAsync();
        public Task<QuestionDTO> GetByIdAsync(GetByIdQuestionDTO dto);
        public Task<QuestionDTO> AddAsync(CreateQuestionDTO dto);
        public Task<QuestionDTO> UpdateAsync(UpdateQuestionDTO dto);
        public Task<QuestionDTO> DeleteAsync(DeleteQuestionDTO dto);
    }
}
