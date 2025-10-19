using AppTech.Business.DTOs.SettingDTOs;

namespace AppTech.Business.Services.InternalServices.Interfaces
{
    public interface ISettingService
    {
        Task<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>>> GetAllAsync(SettingTypeDTO dto);
        Task<SettingDTO> GetByIdAsync(GetByIdSettingDTO dto);
        Task<SettingDTO> AddAsync(CreateSettingDTO dto);
        Task<SettingDTO> UpdateAsync(UpdateSettingDTO dto);
        Task<SettingDTO> DeleteAsync(DeleteSettingDTO dto);
    }

}
