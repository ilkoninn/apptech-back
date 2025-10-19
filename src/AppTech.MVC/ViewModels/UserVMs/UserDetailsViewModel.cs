using AppTech.Core.Entities.Identity;

namespace AppTech.MVC.ViewModels.UserVMs
{
    public class UserDetailsViewModel
    {
        public User User { get; set; }
        public List<ExamResult> ExamResults { get; set; }
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public List<string> UserRoles { get; set; }  
        public List<string> AllRoles { get; set; }
    }
}
