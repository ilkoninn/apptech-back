using AppTech.Business.DTOs.NewsDTOs;

namespace AppTech.Business.Services.Interfaces
{
    public interface INewsTranslationService
    {
        public Task<IQueryable<NewsTranslationDTO>> GetAllAsync();
        public Task<NewsTranslationDTO> GetByIdAsync(GetByIdNewsTranslationDTO dto);
        public Task<NewsTranslationDTO> AddAsync(CreateNewsTranslationDTO dto);
        public Task<NewsTranslationDTO> UpdateAsync(UpdateNewsTranslationDTO dto);
        public Task<NewsTranslationDTO> DeleteAsync(DeleteNewsTranslationDTO dto);
    }
}
