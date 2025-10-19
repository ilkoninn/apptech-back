using AppTech.Business.DTOs.PartnerDTOs;

namespace AppTech.Business.Services.InternalServices.Interfaces
{
    public interface IPartnerService
    {
        public Task<IQueryable<PartnerDTO>> GetAllAsync();
        public Task<PartnerDTO> GetByIdAsync(GetByIdPartnerDTO dto);
        public Task<PartnerDTO> AddAsync(CreatePartnerDTO dto);
        public Task<PartnerDTO> UpdateAsync(UpdatePartnerDTO dto);
        public Task<PartnerDTO> DeleteAsync(DeletePartnerDTO dto);
        public Task<PartnerDTO> AddPartnerOnlineAsync(CreatePartnerOnlineDTO dto);
    }
}
