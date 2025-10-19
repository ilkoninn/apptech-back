using AppTech.Business.DTOs.FAQDTOs;

namespace AppTech.Business.Services.Interfaces
{
    public interface IFAQService
    {
        public Task<IQueryable<FAQDTO>> GetAllAsync(GetAllFAQDTO dto);
        public Task<FAQDTO> GetByIdAsync(GetByIdFAQDTO dto);
        public Task<FAQDTO> AddAsync(CreateFAQDTO dto);
        public Task<FAQDTO> UpdateAsync(UpdateFAQDTO dto);
        public Task<FAQDTO> DeleteAsync(DeleteFAQDTO dto);
    }
}
