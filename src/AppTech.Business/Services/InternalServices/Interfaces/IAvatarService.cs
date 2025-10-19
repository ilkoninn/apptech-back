using AppTech.Business.DTOs.AvatarDTOs;

namespace AppTech.Business.Services.InternalServices.Interfaces
{
    public interface IAvatarService
    {
        public Task<IQueryable<AvatarDTO>> GetAllAsync();
        public Task<AvatarDTO> GetByIdAsync(GetByIdAvatarDTO dto);
        public Task<AvatarDTO> AddAsync(CreateAvatarDTO dto);
        public Task<AvatarDTO> UpdateAsync(UpdateAvatarDTO dto);
        public Task<AvatarDTO> DeleteAsync(DeleteAvatarDTO dto);
    }
}
