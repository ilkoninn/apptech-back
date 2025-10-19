using AppTech.Business.DTOs.ContactUsDTOs;

namespace AppTech.Business.Services.Interfaces
{
    public interface IContactUsService
    {
        public Task<IQueryable<ContactUsDTO>> GetAllAsync();
        public Task<ContactUsDTO> GetByIdAsync(GetByIdContactUsDTO dto);
        public Task<ContactUsDTO> AddAsync(CreateContactUsDTO dto);
        public Task<ContactUsDTO> DeleteAsync(DeleteContactUsDTO dto);
    }
}
