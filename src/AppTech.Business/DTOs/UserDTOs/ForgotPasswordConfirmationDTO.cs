namespace AppTech.Business.DTOs.UserDTOs
{
    public class ForgotPasswordConfirmationDTO
    {
        public string email { get; set; }
        public int number { get; set; }
    }

    public class ForgotPasswordConfirmationResponseDTO
    {
        public string token { get; set; }
    }
}
