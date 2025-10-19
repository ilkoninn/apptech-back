namespace AppTech.Business.DTOs.UserDTOs
{
    public class ResetPasswordUserDTO
    {
        public string newPassword { get; set; }
        public string confirmNewPassword { get; set; }
        public string email { get; set; }
        public string token { get; set; }
    }
}
