using AppTech.Business.DTOs.NewsDTOs;

namespace AppTech.Business.Services.Interfaces
{
    public interface INewsService
    {
        public Task<IQueryable<NewsDTO>> GetAllAsync(GetAllNewsDTO dto);
        public Task<NewsDTO> GetByIdAsync(GetByIdNewsDTO dto);
        public Task<NewsDTO> AddAsync(CreateNewsDTO dto);
        public Task<NewsDTO> UpdateAsync(UpdateNewsDTO dto);
        public Task<NewsDTO> DeleteAsync(DeleteNewsDTO dto);
    }
}
