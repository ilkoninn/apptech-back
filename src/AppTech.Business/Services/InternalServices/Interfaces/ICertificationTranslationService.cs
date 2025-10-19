using AppTech.Business.DTOs.CertificationDTOs;

namespace AppTech.Business.Services.Interfaces
{
    public interface ICertificationTranslationService
    {
        Task<IQueryable<CertificationTranslationDTO>> GetAllAsync();
        Task<CertificationTranslationDTO> GetByIdAsync(GetByIdCertificationTranslationDTO dto);
        Task<CertificationTranslationDTO> AddAsync(CreateCertificationTranslationDTO dto);
        Task<CertificationTranslationDTO> UpdateAsync(UpdateCertificationTranslationDTO dto);
        Task<CertificationTranslationDTO> DeleteAsync(DeleteCertificationTranslationDTO dto);
    }
}
