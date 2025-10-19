using AppTech.Business.DTOs.CompanyDTOs;

namespace AppTech.Business.Services.Interfaces
{
    public interface ICompanyTranslationService
    {
        Task<IQueryable<CompanyTranslationDTO>> GetAllAsync();
        Task<CompanyTranslationDTO> GetByIdAsync(GetByIdCompanyTranslationDTO dto);
        Task<CompanyTranslationDTO> AddAsync(CreateCompanyTranslationDTO dto);
        Task<CompanyTranslationDTO> UpdateAsync(UpdateCompanyTranslationDTO dto);
        Task<CompanyTranslationDTO> DeleteAsync(DeleteCompanyTranslationDTO dto);

    }
}
