using System.Globalization;
using AppTech.Business.DTOs.ExamPackageDTOs;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Business.Services.Interfaces;
using AppTech.Business.Services.InternalServices.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Entities.Identity;
using AppTech.DAL.Repositories.Interfaces;
using AppTech.MVC.ViewModels.ExamPackageVMs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppTech.MVC.Controllers
{
    public class ExamPackageController : BaseController
    {
        private readonly IExamPackageRepository _examPackageRepository;
        private readonly IExamResultRepository _examResultRepository;
        private readonly IExamPackageService _examPackageService;
        private readonly IFileManagerService _fileManagerService;
        private readonly IAccountService _accountService;
        private readonly UserManager<User> _userManager;
        private readonly IExamService _examService;

        public ExamPackageController(
            IExamPackageService examPackageService,
            IExamPackageRepository examPackageRepository,
            IExamService examService,
            UserManager<User> userManager,
            IExamResultRepository examResultRepository,
            IAccountService accountService,
            IFileManagerService fileManagerService)
        {
            _examPackageRepository = examPackageRepository;
            _examResultRepository = examResultRepository;
            _examPackageService = examPackageService;
            _examService = examService;
            _userManager = userManager;
            _accountService = accountService;
            _fileManagerService = fileManagerService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 8)
        {
            try
            {
                var query = await (User.IsInRole("Admin")
                    ? _examPackageRepository.GetAllAsync(x => true)
                    : _examPackageRepository.GetAllAsync(x => !x.IsDeleted));

                var count = query.Count();
                var selectedQuery = query
                    .OrderByDescending(t => t.UpdatedOn)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .AsQueryable();

                var totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var data = new PaginatedQueryable<ExamPackage>(selectedQuery, pageIndex, totalPages);

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading exam packages: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var exams = await _examService.GetAllAsync();
                ViewBag.Exams = new SelectList(exams, "Id", "Code");

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while preparing the creation form: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateExamPackageDTO dto, string price)
        {
            var exams = await _examService.GetAllAsync();

            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Exams = new SelectList(exams, "Id", "Code");
                    TempData["ErrorMessage"] = "Invalid data provided.";
                    return View(dto);
                }

                if (dto.ExamIds == null || !dto.ExamIds.Any())
                {
                    ViewBag.Exams = new SelectList(exams, "Id", "Title");
                    ModelState.AddModelError("ExamIds", "Please select at least one exam.");
                    TempData["ErrorMessage"] = "No exams selected.";
                    return View(dto);
                }

                var decimalBalance = Convert.ToDecimal(price, CultureInfo.InvariantCulture);
                dto.Price = decimalBalance;

                await _examPackageService.AddAsync(dto);
                TempData["SuccessMessage"] = "Exam package created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the exam package: \n{ex.Message}\n{ex.InnerException?.Message}";
                ViewBag.Exams = new SelectList(exams, "Id", "Code");

                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id, int pageIndex = 1)
        {
            try
            {
                var examPackage = await _examPackageRepository.GetByIdAsync(x => x.Id == id, x => x.Exams);

                if (examPackage == null)
                {
                    TempData["ErrorMessage"] = "Exam package not found.";
                    return RedirectToAction("Index");
                }

                var exams = (await _examService.GetAllAsync()).ToList();
                ViewBag.Exams = new SelectList(exams, "Id", "Code");

                var model = new UpdateExamPackageDTO
                {
                    Id = examPackage.Id,
                    Title = examPackage.Title,
                    Price = examPackage.Price,
                    ExamIds = examPackage.Exams.Select(e => e.Id).ToList(),
                    DashImageUrl = examPackage.ImageUrl,
                    SpecificDomain = examPackage.SpecificDomain,
                    PageIndex = pageIndex
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the update form: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateExamPackageDTO dto, string price)
        {
            var exams = await _examService.GetAllAsync();

            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Exams = new MultiSelectList(exams, "Id", "Title", dto.ExamIds);
                    TempData["ErrorMessage"] = "Invalid data provided.";
                    return View(dto);
                }

                var decimalBalance = Convert.ToDecimal(price, CultureInfo.InvariantCulture);
                dto.Price = decimalBalance;

                await _examPackageService.UpdateAsync(dto);
                TempData["SuccessMessage"] = "Exam package updated successfully.";
                return RedirectToAction("Index", new { pageIndex = dto.PageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the exam package: \n{ex.Message}\n{ex.InnerException?.Message}";
                ViewBag.Exams = new MultiSelectList(exams, "Id", "Title", dto.ExamIds);

                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, int pageIndex = 1)
        {
            try
            {
                await _examPackageService.DeleteAsync(new DeleteExamPackageDTO() { Id = id });
                TempData["SuccessMessage"] = "Exam package deleted successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the exam package: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _examPackageRepository.GetByIdAsync(x => x.Id == id, e => e.Exams);

                if (entity == null)
                {
                    TempData["ErrorMessage"] = "Exam package not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                var oldImageUrl = entity.ImageUrl;

                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    var oldFileName = oldImageUrl.Split("/").LastOrDefault();
                    var filePath = Path.Combine("C:/inetpub/wwwroot/backend2/wwwroot/uploads/", oldFileName);

                    _fileManagerService.DeleteFile(filePath);
                }

                await _examPackageRepository.RemoveAsync(entity);

                TempData["SuccessMessage"] = "Exam package removed successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while removing the exam package: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Recover(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _examPackageRepository.GetByIdAsync(x => x.Id == id);

                if (entity == null)
                {
                    TempData["ErrorMessage"] = "Exam package not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                await _examPackageRepository.RecoverAsync(entity);
                TempData["SuccessMessage"] = "Exam package recovered successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while recovering the exam package: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        // Other Methods
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddPackagesToPartnerUsers()
        {
            try
            {
                var examPackages = (await _examPackageRepository.GetAllAsync(x => !x.IsDeleted)).ToList();

                ViewBag.ExamPackages = new SelectList(examPackages, "Id", "Title");

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while preparing the form: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddPackagesToPartnerUsers(AddPackageToUsersVM vm)
        {
            try
            {
                if (string.IsNullOrEmpty(vm.UsernameOrEmail))
                {
                    ModelState.AddModelError("", "Username or email is required.");
                    TempData["ErrorMessage"] = "Username or email is required.";
                    return View(vm);
                }

                var user = await _accountService.CheckNotFoundForLoginByUsernameOrEmailAsync(vm.UsernameOrEmail);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index");
                }

                var examPackage = await _examPackageRepository.GetByIdAsync(x => x.Id == vm.ExamPackageId, e => e.Exams);

                foreach (var exam in examPackage.Exams)
                {
                    var newExamResult = new ExamResult
                    {
                        UserId = user.Id,
                        ExamId = exam.Id,
                    };

                    await _examResultRepository.AddAsync(newExamResult);
                }

                _accountService.CheckIdentityResult(await _userManager.UpdateAsync(user));

                TempData["SuccessMessage"] = "Exam packages successfully assigned to the partner users.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while assigning the exam packages: \n{ex.Message}\n{ex.InnerException?.Message}";
                return View(vm);
            }
        }
    }
}
