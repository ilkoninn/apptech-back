using System.Security.Claims;

namespace AppTech.Business.DTOs.GoogleDTOs
{
    public class RegisterGoogleUserDTO
    {
        public string IpAddress { get; set; }
        public string Lang { get; set; }
        public ClaimsPrincipal Principal { get; set; }
    }
    public class RegisterGoogleUserResponseDTO
    {
        public int? statusCode { get; set; }
        public string? message { get; set; }
        public string? token { get; set; }
        public string? redirectUrl { get; set; }
    }
}
