using System.Globalization;
using AppTech.Business.DTOs.CertificationDTOs;
using AppTech.Business.DTOs.CompanyDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Entities.Identity;
using AppTech.DAL.Repositories.Abstractions;
using AppTech.DAL.Repositories.Interfaces;
using AppTech.MVC.ViewModels.UserVMs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppTech.MVC.Controllers
{
    public class CertificationController : BaseController
    {
        private readonly ICertificationTranslationRepository _certificationTranslationRepository;
        private readonly ICertificationTranslationService _certificationTranslationService;
        private readonly ICertificationUserRepository _certificationUserRepository;
        private readonly ICertificationRepository _certificationRepository;
        private readonly ICertificationService _certificationService;
        private readonly IFileManagerService _fileManagerService;
        private readonly ICompanyService _companyService;
        private readonly IAccountService _accountService;
        private readonly UserManager<User> _userManager;

        public CertificationController(
            ICertificationService certificationService,
            ICertificationTranslationService certificationTranslationService,
            ICompanyService companyService,
            ICertificationRepository certificationRepository,
            ICertificationTranslationRepository certificationTranslationRepository,
            IFileManagerService fileManagerService,
            IAccountService accountService,
            UserManager<User> userManager,
            ICertificationUserRepository certificationUserRepository)
        {
            _certificationTranslationRepository = certificationTranslationRepository;
            _certificationTranslationService = certificationTranslationService;
            _certificationUserRepository = certificationUserRepository;
            _certificationRepository = certificationRepository;
            _certificationService = certificationService;
            _fileManagerService = fileManagerService;
            _companyService = companyService;
            _accountService = accountService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 8)
        {
            try
            {
                var query = await (User.IsInRole("Admin")
                    ? _certificationRepository.GetAllAsync(x => true)
                    : _certificationRepository.GetAllAsync(x => !x.IsDeleted));

                var count = query.Count();
                var selectedQuery = query
                    .OrderByDescending(t => t.UpdatedOn)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .AsQueryable();

                var totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var data = new PaginatedQueryable<Certification>(selectedQuery, pageIndex, totalPages);

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading certifications: \n{ex.Message}\n\n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var companies = await _companyService.GetAllAsync(new GetAllCompanyDTO());
                ViewBag.Companies = companies.ToList();

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while preparing the create form: \n{ex.Message}\n\n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCertificationDTO dto, string price, string discountPrice)
        {
            var companies = await _companyService.GetAllAsync(new GetAllCompanyDTO());
            var decimalBalance = Convert.ToDecimal(price, CultureInfo.InvariantCulture);
            dto.Price = decimalBalance;
            var decimalDisBalance = Convert.ToDecimal(discountPrice, CultureInfo.InvariantCulture);
            dto.DiscountPrice = decimalDisBalance;

            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Companies = companies.ToList();
                    TempData["ErrorMessage"] = "Please correct the form errors.";
                    return View(dto);
                }

                var createdCertification = await _certificationService.AddAsync(dto);

                if (dto.Translations != null && dto.Translations.Any())
                {
                    foreach (var translation in dto.Translations)
                    {
                        if (translation.Description == null) continue;

                        translation.CertificationId = createdCertification.Id;
                        await _certificationTranslationService.AddAsync(translation);
                    }
                }

                TempData["SuccessMessage"] = "Certification created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the certification: \n{ex.Message}\n{ex.InnerException?.Message}";
                ViewBag.Companies = companies.ToList();

                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id, int pageIndex = 1)
        {
            try
            {
                var certification = await _certificationService.GetByIdAsync(new GetByIdCertificationDTO() { Id = id });

                if (certification == null)
                {
                    TempData["ErrorMessage"] = "Certification not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                var translations = certification.Translations?.Select(translation => new UpdateCertificationTranslationDTO
                {
                    Id = translation?.Id ?? 0,
                    CertificationId = certification.Id,
                    Description = translation?.Description ?? string.Empty,
                    Language = LanguageChanger.Change(translation.Language)
                }).ToList() ?? new List<UpdateCertificationTranslationDTO>();

                var companies = await _companyService.GetAllAsync(new GetAllCompanyDTO());
                ViewBag.Companies = companies;

                var model = new UpdateCertificationDTO
                {
                    Id = certification.Id,
                    LastVersion = certification.LastVersion,
                    CompanyId = certification.CompanyId,
                    SubTitle = certification.SubTitle,
                    Price = certification.Price,
                    DiscountPrice = certification.DiscountPrice,
                    IsTrend = certification.IsTrend,
                    Translations = translations,
                    Code = certification.Code,
                    DashImageUrl = certification.ImageUrl,
                    PageIndex = pageIndex
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the certification details: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateCertificationDTO dto, string removedTranslationIds,
            string price, string discountPrice)
        {
            var companies = await _companyService.GetAllAsync(new GetAllCompanyDTO());
            var decimalBalance = Convert.ToDecimal(price, CultureInfo.InvariantCulture);
            dto.Price = decimalBalance;
            var decimalDisBalance = Convert.ToDecimal(discountPrice, CultureInfo.InvariantCulture);
            dto.DiscountPrice = decimalDisBalance;

            try
            {
                var updatedCertification = await _certificationService.UpdateAsync(dto);

                if (!string.IsNullOrEmpty(removedTranslationIds))
                {
                    var idsToRemove = removedTranslationIds.Split(',').Select(int.Parse).ToList();

                    foreach (var id in idsToRemove)
                    {
                        await _certificationTranslationRepository.RemoveAsync(
                            await _certificationTranslationRepository.GetByIdAsync(x => x.Id == id));
                    }
                }

                if (dto.Translations is not null)
                {
                    foreach (var translation in dto.Translations)
                    {
                        if (translation.Description == null) continue;

                        if (translation.Id == 0)
                        {
                            var createTranslationDTO = new CreateCertificationTranslationDTO
                            {
                                CertificationId = updatedCertification.Id,
                                Description = translation.Description,
                                Language = translation.Language
                            };

                            await _certificationTranslationService.AddAsync(createTranslationDTO);
                        }
                        else
                        {
                            var updateTranslationDTO = new UpdateCertificationTranslationDTO
                            {
                                Id = translation.Id,
                                CertificationId = updatedCertification.Id,
                                Description = translation.Description,
                                Language = translation.Language
                            };

                            await _certificationTranslationService.UpdateAsync(updateTranslationDTO);
                        }
                    }
                }

                TempData["SuccessMessage"] = "Certification updated successfully.";
                return RedirectToAction("Index", new { pageIndex = dto.PageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the certification: \n{ex.Message}\n{ex.InnerException?.Message}";
                ViewBag.Companies = companies;


                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, int pageIndex = 1)
        {
            try
            {
                await _certificationService.DeleteAsync(new DeleteCertificationDTO() { Id = id });
                TempData["SuccessMessage"] = "Certification deleted successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the certification: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _certificationRepository.GetByIdAsync(x => x.Id == id);

                if (entity == null)
                {
                    TempData["ErrorMessage"] = "Certification not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                var oldImageUrl = entity.ImageUrl;

                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    var oldFileName = oldImageUrl.Split("/").LastOrDefault();
                    var filePath = Path.Combine("C:/inetpub/wwwroot/backend2/wwwroot/uploads/", oldFileName);

                    _fileManagerService.DeleteFile(filePath);
                }

                await _certificationRepository.RemoveAsync(entity);

                TempData["SuccessMessage"] = "Certification removed successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while removing the certification: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Recover(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _certificationRepository.GetByIdAsync(x => x.Id == id);
                await _certificationRepository.RecoverAsync(entity);

                TempData["SuccessMessage"] = "Certification recovered successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while recovering the certification: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        // Supportive Methods
        [HttpGet]
        public async Task<IActionResult> SendCertificationToUser()
        {
            var userCertificationVM = new UserCertificationViewModel()
            {
                Certifications = await _certificationRepository.GetAllAsync(x => !x.IsDeleted),
                Users = await _userManager.Users.ToListAsync()
            };

            return View(userCertificationVM);
        }

        [HttpPost]
        public async Task<IActionResult> SendCertificationToUser(UserCertificationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = (await _accountService.CheckNotFoundForLoginByUsernameOrEmailAsync(model.UsernameOrEmail)).Id;

                var newCertificationUser = new CertificationUser()
                {
                    CertificationId = model.CertificationId,
                    UserId = userId,
                };

                await _certificationUserRepository.AddAsync(newCertificationUser);

                TempData["SuccessMessage"] = "Certification sent successfully to selected users.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while sending the certification: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> RemoveCertificationFromUser()
        {
            var userCertificationVM = new UserCertificationViewModel()
            {
                Certifications = await _certificationRepository.GetAllAsync(x => !x.IsDeleted),
                Users = await _userManager.Users.ToListAsync()
            };

            return View(userCertificationVM);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCertificationFromUser(UserCertificationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _accountService.CheckNotFoundForLoginByUsernameOrEmailAsync(model.UsernameOrEmail);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return View(model);
                }

                var userCertifications = await _certificationUserRepository.GetAllAsync(
                    x => x.UserId == user.Id && !x.IsDeleted,
                    c => c.Certification
                );

                if (userCertifications != null && userCertifications.Any())
                {
                    model.Certifications = userCertifications.Select(x => x.Certification).ToList();
                }
                else
                {
                    TempData["ErrorMessage"] = "The user does not have any certifications.";
                }

                if (model.CertificationId != 0)
                {
                    var certificationUser = userCertifications.FirstOrDefault(x => x.CertificationId == model.CertificationId);
                    if (certificationUser != null)
                    {
                        await _certificationUserRepository.RemoveAsync(certificationUser);
                        TempData["SuccessMessage"] = "Certification removed successfully from the user.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "The user does not have the specified certification.";
                    }

                    return RedirectToAction(nameof(RemoveCertificationFromUser));
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while processing the request: " + ex.Message;
                return View(model);
            }
        }
    }
}
