namespace AppTech.Business.DTOs.UserDTOs
{
    public class ChangePasswordUserDTO
    {
        public string currentPassword { get; set; }
        public string newPassword { get; set; }
    }

    public class ChangePasswordResponseDTO
    {
        public string Message { get; set; }
    }
}
