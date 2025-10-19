namespace AppTech.Business.DTOs.UserDTOs
{
    public class RegisterUserDTO
    {
        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
    }

    public class RegisterUserResponseDTO
    {
        public string email { get; set; }
    }
}
