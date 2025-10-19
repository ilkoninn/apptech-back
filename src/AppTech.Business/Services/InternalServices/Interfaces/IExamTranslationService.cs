using AppTech.Business.DTOs.ExamDTOs;
using AppTech.Core.Entities;

namespace AppTech.Business.Services.Interfaces
{
    public interface IExamTranslationService
    {
        public Task<IQueryable<ExamTranslationDTO>> GetAllAsync();
        public Task<ExamTranslation> GetByIdAsync(GetByIdExamTranslationDTO dto);
        public Task<ExamTranslationDTO> AddAsync(CreateExamTranslationDTO dto);
        public Task<ExamTranslationDTO> UpdateAsync(UpdateExamTranslationDTO dto);
        public Task<ExamTranslationDTO> DeleteAsync(DeleteExamTranslationDTO dto);
    }
}
