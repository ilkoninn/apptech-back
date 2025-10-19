using AppTech.Business.DTOs.UserDTOs;
using AppTech.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace AppTech.Business.Services.Interfaces
{
    public interface IAccountService
    {
        // Registration Methods
        Task<RegisterUserResponseDTO> RegisterAsync(RegisterUserDTO dto);
        Task EmailConfirmationAsync(ConfirmEmailDTO dto);
        Task ClickToResendAsync(ClickToResendDTO dto);

        // Login Methods
        Task<ForgotPasswordConfirmationResponseDTO> ForgotPasswordConfirmationAsync(ForgotPasswordConfirmationDTO dto);
        Task CheckUserPasswordAsync(User user, string password);
        Task<LoginUserResponseDTO> LoginAsync(LoginUserDTO dto);
        Task ForgotPasswordAsync(ForgotPasswordUserDTO dto);
        Task ResetPasswordAsync(ResetPasswordUserDTO dto);
        Task LogoutAsync(LogoutDTO dto);

        // Account Methods
        Task<ChangePasswordResponseDTO> ChangePasswordAsync(ChangePasswordUserDTO dto, string userId);
        Task<UpdateUserResponseDTO> Update(UpdateUserDTO dto, string userId);
        Task ConfirmUpdateEmailAsync(ConfirmUpdateEmailDTO dto);

        // Supportive Methods
        Task<User> CheckNotFoundForLoginByUsernameOrEmailAsync(string userNameOrEmail);
        void ChangeDefaultIndetityErrorDescriber(string lang);
        Task<bool> CheckUserStatus(string userNameOrEmail);
        Task<User> CheckNotFoundByIdAsync(string userId);
        void CheckIdentityResult(IdentityResult result);
        Task<string> GetUserRoleAsync(User user);
        Task ChangeUserStatus(string userId);
    }
}
