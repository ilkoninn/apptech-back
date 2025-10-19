using AppTech.Business.DTOs.CertificationDTOs;

namespace AppTech.Business.Services.Interfaces
{
    public interface ICertificationService
    {
        Task<IQueryable<CertificationDTO>> GetAllAsync(GetAllCertificationDTO dto);
        Task<IQueryable<CertificationUserResponseDTO>> GetAllByUserAsync(GetAllByUserDTO dto);
        Task<CertificationDTO> GetByIdAsync(GetByIdCertificationDTO dto);
        Task<CertificationDTO> GetBySlugAsync(GetBySlugCertificationDTO dto);
        Task<CertificationDashboardDTO> GetBySlugByUserAsync(GetBySlugCertificationDTO dto);
        Task<CertificationDTO> AddAsync(CreateCertificationDTO dto);
        Task<CertificationDTO> UpdateAsync(UpdateCertificationDTO dto);
        Task<CertificationDTO> DeleteAsync(DeleteCertificationDTO dto);
    }
}
