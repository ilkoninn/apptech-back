using AppTech.Business.DTOs.GoogleDTOs;

namespace AppTech.Business.Services.ExternalServices.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<RegisterGoogleUserResponseDTO> RegisterGoogleAccountAsync(RegisterGoogleUserDTO dto);
    }
}
