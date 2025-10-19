using AppTech.Core.Entities.Identity;
using AppTech.DAL.Repositories.Interfaces;
using AppTech.MVC.ViewModels.HomeVMs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppTech.MVC.Controllers
{
    public class HomeController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly ICertificationRepository _certificationRepository;
        private readonly ICertificationUserRepository _certificationUserRepository;
        private readonly DateTime _today;
        private readonly IContactUsRepository _contactUsRepository;

        public HomeController(UserManager<User> userManager,
            ICertificationRepository certificationRepository,
            ICertificationUserRepository certificationUserRepository,
            IContactUsRepository contactUsRepository)
        {
            _userManager = userManager;
            _certificationRepository = certificationRepository;
            _certificationUserRepository = certificationUserRepository;
            _today = DateTime.Today;
            _contactUsRepository = contactUsRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _contactUsRepository.GetAllAsync(x => x.IsDeleted!);
            result = result.Take(5).ToList();
            var vm = new HomeVM()
            {
                UsersCount = _userManager.Users.Count(),
                TodayCertifications = await _certificationUserRepository.GetAllAsync(x => x.CreatedOn >= _today && x.CreatedOn < _today.AddDays(1)),
                OnExamCount = await _userManager.Users.Where(x => x.OnExam == true).ToListAsync(),
                OnOnlineCount = _userManager.Users.Where(x => x.IsOnline == true).Count(),
                UserName = User.Identity.Name,
                ContactUsMessages = result.Select(x => x.Message).ToList()

            };


            return View(vm);
        }

        [HttpGet]
        public IActionResult AccessDeniedPage()
        {
            return View();
        }

        [Route("Error/{statusCode}")]
        public IActionResult Error(int? statusCode)
        {
            if (statusCode.HasValue)
            {
                if (statusCode == 404)
                {
                    return View("NotFound");
                }
                else
                {
                    return View("GenError");
                }

            }
            return View();
        }

        [HttpGet]
        public IActionResult NotFound()
        {
            return View();
        }
    }
}
