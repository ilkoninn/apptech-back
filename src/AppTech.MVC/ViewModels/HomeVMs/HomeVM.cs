using AppTech.Core.Entities.Identity;
using AppTech.Core.Entities;

namespace AppTech.MVC.ViewModels.HomeVMs
{
    public class HomeVM
    {
        public string? UserName { get; set; }
        public int? UsersCount { get; set; }
        public int? CertificationCount { get; set; }
        public List<User>? OnExamCount { get; set; }
        public int? OnOnlineCount { get; set; }
        public List<string> ContactUsMessages { get; set; }
        public ICollection<CertificationUser> TodayCertifications { get; set; }
    }
}
