using AppTech.Core.Entities.Identity;

namespace AppTech.MVC.ViewModels.UserVMs
{
    public class UserRolesViewModel
    {
        public User User { get; set; }
        public List<string> Roles { get; set; }
    }
}
