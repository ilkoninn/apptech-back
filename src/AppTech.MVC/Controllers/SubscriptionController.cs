using System.Globalization;
using AppTech.Business.DTOs.SubscriptionDTOs;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Entities.Identity;
using AppTech.DAL.Repositories.Abstractions;
using AppTech.DAL.Repositories.Interfaces;
using AppTech.MVC.ViewModels.SubscriptionVMs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppTech.MVC.Controllers
{
    public class SubscriptionController : BaseController
    {
        private readonly ICertificationUserRepository _certificationUserRepository;
        private readonly ISubscriptionUserRepository _subscriptionUserRepository;
        private readonly ICertificationRepository _certificationRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IExamResultRepository _examResultRepository;
        private readonly IFileManagerService _fileManagerService;
        private readonly UserManager<User> _userManager;

        public SubscriptionController(
            ISubscriptionRepository subscriptionRepository,
            ISubscriptionUserRepository subscriptionUserRepository,
            UserManager<User> userManager,
            IFileManagerService fileManagerService,
            ICertificationRepository certificationRepository,
            ICertificationUserRepository certificationUserRepository,
            IExamResultRepository examResultRepository)
        {
            _certificationUserRepository = certificationUserRepository;
            _subscriptionUserRepository = subscriptionUserRepository;
            _certificationRepository = certificationRepository;
            _subscriptionRepository = subscriptionRepository;
            _examResultRepository = examResultRepository;
            _fileManagerService = fileManagerService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 8)
        {
            try
            {
                var query = await (User.IsInRole("Admin")
                    ? _subscriptionRepository.GetAllAsync(x => true, u => u.Certification)
                    : _subscriptionRepository.GetAllAsync(x => !x.IsDeleted, u => u.Certification));

                var count = query.Count();
                var selectedQuery = query
                    .OrderByDescending(t => t.UpdatedOn)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .AsQueryable();

                var totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var data = new PaginatedQueryable<Subscription>(selectedQuery, pageIndex, totalPages);

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading subscriptions: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                ViewBag.Certifications = await _certificationRepository.GetAllAsync(x => !x.IsDeleted);

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the form: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSubscriptionDTO dto, string price)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Certifications = await _certificationRepository.GetAllAsync(x => !x.IsDeleted);

                    TempData["ErrorMessage"] = "Invalid data provided.";
                    return View(dto);
                }

                var decimalBalance = Convert.ToDecimal(price, CultureInfo.InvariantCulture);
                dto.Price = decimalBalance;

                var imageUrl = await _fileManagerService.UploadFileAsync(dto.Image);

                var newSubscription = new Subscription
                {
                    CertificationId = dto.CertificationId,
                    Price = dto.Price,
                    SpecificDomain = dto.SpecificDomain,
                    ImageUrl = imageUrl,
                };

                await _subscriptionRepository.AddAsync(newSubscription);
                TempData["SuccessMessage"] = "Subscription created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Certifications = await _certificationRepository.GetAllAsync(x => !x.IsDeleted);

                TempData["ErrorMessage"] = $"An error occurred while creating the subscription: \n{ex.Message}\n{ex.InnerException?.Message}";
                return View(dto);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Update(int id, int pageIndex = 1)
        {
            try
            {
                var subscription = await _subscriptionRepository.GetByIdAsync(x => x.Id == id);

                if (subscription == null)
                {
                    TempData["ErrorMessage"] = "Subscription not found.";
                    return RedirectToAction("Index");
                }

                ViewBag.Certifications = await _certificationRepository.GetAllAsync(x => !x.IsDeleted);

                var model = new UpdateSubscriptionDTO
                {
                    Id = subscription.Id,
                    SpecificDomain = subscription.SpecificDomain,
                    Price = subscription.Price,
                    DashImageUrl = subscription.ImageUrl,
                    CertificationId = subscription.CertificationId, 
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
        public async Task<IActionResult> Update(UpdateSubscriptionDTO dto, string price)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Certifications = await _certificationRepository.GetAllAsync(x => !x.IsDeleted);

                    TempData["ErrorMessage"] = "Invalid data provided.";
                    return View(dto);
                }

                var decimalBalance = Convert.ToDecimal(price, CultureInfo.InvariantCulture);
                dto.Price = decimalBalance;

                var oldSubscription = await _subscriptionRepository.GetByIdAsync(x => !x.IsDeleted && x.Id == dto.Id);

                oldSubscription.Price = dto.Price;
                oldSubscription.SpecificDomain = dto.SpecificDomain;
                oldSubscription.CertificationId = dto.CertificationId;

                if (dto.Image != null)
                {
                    oldSubscription.ImageUrl = await _fileManagerService.UploadFileAsync(dto.Image);
                }

                await _subscriptionRepository.UpdateAsync(oldSubscription);
                TempData["SuccessMessage"] = "Subscription updated successfully.";
                return RedirectToAction("Index", new { pageIndex = dto.PageIndex });
            }
            catch (Exception ex)
            {
                ViewBag.Certifications = await _certificationRepository.GetAllAsync(x => !x.IsDeleted);

                TempData["ErrorMessage"] = $"An error occurred while updating the subscription: \n{ex.Message}\n{ex.InnerException?.Message}";
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, int pageIndex = 1)
        {
            try
            {
                await _subscriptionRepository.DeleteAsync(
                    await _subscriptionRepository.GetByIdAsync(x => x.Id == id));

                TempData["SuccessMessage"] = "Subscription deleted successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the subscription: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _subscriptionRepository.GetByIdAsync(x => x.Id == id);

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

                await _subscriptionRepository.RemoveAsync(entity);

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
                var subscription = await _subscriptionRepository.GetByIdAsync(x => x.Id == id);

                if (subscription == null)
                {
                    TempData["ErrorMessage"] = "Subscription not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                await _subscriptionRepository.RecoverAsync(subscription);

                TempData["SuccessMessage"] = "Subscription recovered successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while recovering the subscription: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddSubscriptionToUser()
        {
            try
            {
                ViewBag.Subscriptions = await _subscriptionRepository.GetAllAsync(x => !x.IsDeleted, c => c.Certification);


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
        public async Task<IActionResult> AddSubscriptionToUser(AddSubscriptionToUsersVM vm)
        {
            try
            {
                if (string.IsNullOrEmpty(vm.UsernameOrEmail))
                {
                    ModelState.AddModelError("", "Username or email is required.");
                    TempData["ErrorMessage"] = "Username or email is required.";
                    return View(vm);
                }

                var user = await _userManager.FindByEmailAsync(vm.UsernameOrEmail) ?? await _userManager.FindByNameAsync(vm.UsernameOrEmail);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index");
                }

                var subscription = await _subscriptionRepository.GetByIdAsync(x => x.Id == vm.SubscriptionId,
                    c => c.Certification,
                    e => e.Certification.Exams);

                var newSubscriptionUser = new SubscriptionUser
                {
                    UserId = user.Id,
                    SubscriptionId = subscription.Id,
                    ExpiredOn = DateTime.UtcNow.AddMonths(1),
                };

                await _subscriptionUserRepository.AddAsync(newSubscriptionUser);

                var newCertificationUser = new CertificationUser
                {
                    CertificationId = subscription.CertificationId,
                    UserId = user.Id,
                };

                await _certificationUserRepository.AddAsync(newCertificationUser);

                var newExamResult = new ExamResult
                {
                    UserId = user.Id,
                    ExamId = subscription.Certification.Exams.FirstOrDefault().Id,   
                };

                await _examResultRepository.AddAsync(newExamResult);

                TempData["SuccessMessage"] = "Subscriptions successfully assigned to the user.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while assigning the subscription: \n{ex.Message}\n{ex.InnerException?.Message}";
                return View(vm);
            }
        }
    }
}
