using AppTech.Business.DTOs.PromotionDTOs;

namespace AppTech.Business.Services.Interfaces
{
    public interface IPromotionService
    {
        public Task<PromotionDTO> GetByIdAsync(GetByIdPromotionDTO dto);
        public Task<IQueryable<PromotionDTO>> GetAllAsync();
        public Task<PromotionDTO> AddAsync(CreatePromotionDTO dto);
        public Task<PromotionDTO> UpdateAsync(UpdatePromotionDTO dto);
        public Task<PromotionDTO> DeleteAsync(int id);
    }
}
