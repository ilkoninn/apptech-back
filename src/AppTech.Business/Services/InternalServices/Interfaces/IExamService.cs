using AppTech.Business.DTOs.CompanyDTOs;
using AppTech.Business.DTOs.ExamDTOs;
using AppTech.Business.DTOs.UserDTOs;

namespace AppTech.Business.Services.Interfaces
{
    public interface IExamService
    {
        Task<IQueryable<ExamUserResponseDTO>> GetAllExamsByUser(GetAllExamByUser dto);
        Task<IQueryable<ExamResultDTO>> GetAllExamResultsByUserAsync(UserTimerDTO dto);
        Task<ResponseTerminalDTO> StartExamWithDropletAsync(StartTerminalExamDTO dto);
        Task<IQueryable<ExamDTO>> GetAllAsync(GetExamBySlugCompanyDTO dto);
        Task<RandomExamDTO> TakeExamAsync(CreateRandomExamDTO dto);
        Task<ExamResultDTO> CheckExamAsync(SubmitExamDTO dto);
        Task<ExamDTO> GetBySlugAsync(GetBySlugExamDTO dto);
        Task<ExamDTO> GetByIdAsync(GetByIdExamDTO dto);
        Task<ExamDTO> DeleteAsync(DeleteExamDTO dto);
        Task<ExamDTO> UpdateAsync(UpdateExamDTO dto);
        Task<ExamDTO> AddAsync(CreateExamDTO dto);
        Task<IQueryable<ExamDTO>> GetAllAsync();
        Task<bool> CheckExamStatus(string userId);
        Task StartExamAsync(UserTimerDTO dto);
        Task ChangeExamStatus(string userId);
        Task EndExamAsync(UserTimerDTO dto);
    }
}
