using AppTech.Business.DTOs.ExamPackageDTOs;

namespace AppTech.Business.Services.InternalServices.Interfaces
{
    public interface IExamPackageService
    {
        public Task<IQueryable<ExamPackageDTO>> GetAllAsync(GetAllExamPackageDTO dto);
        public Task<ExamPackageDTO> GetByIdAsync(GetByIdExamPackageDTO dto);
        public Task<ExamPackageDTO> AddAsync(CreateExamPackageDTO dto);
        public Task<ExamPackageDTO> UpdateAsync(UpdateExamPackageDTO dto);
        public Task<ExamPackageDTO> DeleteAsync(DeleteExamPackageDTO dto);
    }
}
