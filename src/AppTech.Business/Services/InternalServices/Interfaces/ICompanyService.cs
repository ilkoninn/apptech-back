using AppTech.Business.DTOs.CompanyDTOs;
using AppTech.Core.Entities.Commons;

namespace AppTech.Business.Services.Interfaces
{
    public interface ICompanyService
    {
        Task<IQueryable<CompanyDTO>> GetAllAsync(GetAllCompanyDTO dto);
        Task<PaginatedQueryable<CompanyDTO>> GetAllByPaginationAsync(GetAllCompanyByPageDTO dto);
        Task<CompanyDTO> GetByIdAsync(GetByIdCompanyDTO dto);
        Task<CompanyDTO> GetBySlugAsync(GetBySlugCompanyDTO dto);
        Task<CompanyDTO> AddAsync(CreateCompanyDTO dto);
        Task<CompanyDTO> UpdateAsync(UpdateCompanyDTO dto);
        Task<CompanyDTO> DeleteAsync(DeleteCompanyDTO dto);
    }
}
