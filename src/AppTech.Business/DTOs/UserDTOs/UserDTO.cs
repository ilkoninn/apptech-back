namespace AppTech.Business.DTOs.UserDTOs
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool IsBanned { get; set; }
    }

}
