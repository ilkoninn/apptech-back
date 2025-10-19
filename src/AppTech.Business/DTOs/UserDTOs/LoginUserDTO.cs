namespace AppTech.Business.DTOs.UserDTOs
{
    public class LoginUserDTO
    {
        public string usernameOrEmail { get; set; }
        public string password { get; set; }
        public string ipAddress { get; set; }
    }

    public class LoginUserResponseDTO
    {
        public string token { get; set; }
    }
}
