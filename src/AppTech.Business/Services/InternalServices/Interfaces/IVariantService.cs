using AppTech.Business.DTOs.VariantDTOs;

namespace AppTech.Business.Services.Interfaces
{
    public interface IVariantService
    {
        public Task<IQueryable<VariantDTO>> GetAllAsync();
        public Task<VariantDTO> GetByIdAsync(GetByIdVariantDTO dto);
        public Task<VariantDTO> AddAsync(CreateVariantDTO dto);
        public Task<VariantDTO> UpdateAsync(UpdateVariantDTO dto);
        public Task<VariantDTO> DeleteAsync(DeleteVariantDTO dto);
    }
}
