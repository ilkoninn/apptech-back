using AppTech.Business.DTOs.SettingDTOs;

namespace AppTech.Business.Services.InternalServices.Interfaces
{
    public interface ISettingTranslationService
    {
        Task<IQueryable<SettingTranslationDTO>> GetAllAsync();
        Task<SettingTranslationDTO> GetByIdAsync(GetByIdSettingTranslationDTO dto);
        Task<SettingTranslationDTO> AddAsync(CreateSettingTranslationDTO dto);
        Task<SettingTranslationDTO> UpdateAsync(UpdateSettingTranslationDTO dto);
        Task<SettingTranslationDTO> DeleteAsync(DeleteSettingTranslationDTO dto);
    }
}
