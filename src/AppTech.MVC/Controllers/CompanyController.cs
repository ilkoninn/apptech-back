using AppTech.Business.DTOs.CompanyDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.MVC.Controllers
{
    public class CompanyController : BaseController
    {
        private readonly ICompanyService _companyService;
        private readonly ICompanyRepository _companyRepository;
        private readonly IFileManagerService _fileManagerService;
        private readonly ICompanyTranslationService _companyTranslationService;
        private readonly ICompanyTranslationRepository _companyTranslationRepository;

        public CompanyController(
            ICompanyService companyService,
            ICompanyRepository companyRepository,
            ICompanyTranslationService companyTranslationService,
            ICompanyTranslationRepository companyTranslationRepository,
            IFileManagerService fileManagerService)
        {
            _companyService = companyService;
            _companyRepository = companyRepository;
            _companyTranslationService = companyTranslationService;
            _companyTranslationRepository = companyTranslationRepository;
            _fileManagerService = fileManagerService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 8)
        {
            try
            {
                var query = await (User.IsInRole("Admin")
                    ? _companyRepository.GetAllAsync(x => true)
                    : _companyRepository.GetAllAsync(x => !x.IsDeleted));

                var count = query.Count();
                var selectedQuery = query
                    .OrderByDescending(t => t.UpdatedOn)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .AsQueryable();

                var totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var data = new PaginatedQueryable<Company>(selectedQuery, pageIndex, totalPages);

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading companies: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCompanyDTO dto)
        {
            try
            {
                var createdCompany = await _companyService.AddAsync(dto);

                if (dto.Translations != null && dto.Translations.Any())
                {
                    foreach (var translation in dto.Translations)
                    {
                        if (translation.SecTitle == null || translation.SecDescription == null) continue;

                        translation.CompanyId = createdCompany.Id;
                        await _companyTranslationService.AddAsync(translation);
                    }
                }

                TempData["SuccessMessage"] = "Company created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the company: \n{ex.Message}\n{ex.InnerException?.Message}";
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id, int pageIndex = 1)
        {
            try
            {
                var company = await _companyService.GetByIdAsync(new GetByIdCompanyDTO() { Id = id });

                if (company == null)
                {
                    TempData["ErrorMessage"] = "Company not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                var translations = company.Translations?.Select(translation => new UpdateCompanyTranslationDTO
                {
                    Id = translation?.Id ?? 0,
                    SecTitle = translation?.SecTitle,
                    SecDescription = translation?.SecDescription ?? string.Empty,
                    Language = LanguageChanger.Change(translation.Language)
                }).ToList() ?? new List<UpdateCompanyTranslationDTO>();

                var model = new UpdateCompanyDTO
                {
                    Id = company.Id,
                    IsTop = company.IsTop ?? false,
                    Title = company.Title,
                    Translations = translations,
                    DashImageUrl = company.ImageUrl,
                    PageIndex = pageIndex
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the company details: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateCompanyDTO dto, string removedTranslationIds)
        {
            try
            {
                var updatedCompany = await _companyService.UpdateAsync(dto);

                if (!string.IsNullOrEmpty(removedTranslationIds))
                {
                    var idsToRemove = removedTranslationIds.Split(',').Select(int.Parse).ToList();

                    foreach (var id in idsToRemove)
                    {
                        await _companyTranslationRepository.RemoveAsync(
                            await _companyTranslationRepository.GetByIdAsync(x => x.Id == id));
                    }
                }

                if(dto.Translations is not null)
                {
                    foreach (var translation in dto.Translations)
                    {
                        if (translation.SecTitle == null || translation.SecDescription == null) continue;

                        if (translation.Id == 0)
                        {
                            var createTranslationDTO = new CreateCompanyTranslationDTO
                            {
                                CompanyId = updatedCompany.Id,
                                SecTitle = translation.SecTitle,
                                SecDescription = translation.SecDescription,
                                Language = translation.Language
                            };

                            await _companyTranslationService.AddAsync(createTranslationDTO);
                        }
                        else
                        {
                            var updateTranslationDTO = new UpdateCompanyTranslationDTO
                            {
                                Id = translation.Id,
                                CompanyId = updatedCompany.Id,
                                SecTitle = translation.SecTitle,
                                SecDescription = translation.SecDescription,
                                Language = translation.Language
                            };

                            await _companyTranslationService.UpdateAsync(updateTranslationDTO);
                        }
                    }
                }

                TempData["SuccessMessage"] = "Company updated successfully.";
                return RedirectToAction("Index", new { pageIndex = dto.PageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the company: \n{ex.Message}\n{ex.InnerException?.Message}";
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, int pageIndex = 1)
        {
            try
            {
                await _companyService.DeleteAsync(new DeleteCompanyDTO { Id = id });

                TempData["SuccessMessage"] = "Company deleted successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the company: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _companyRepository.GetByIdAsync(x => x.Id == id);

                if (entity == null)
                {
                    TempData["ErrorMessage"] = "Company not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                var oldImageUrl = entity.ImageUrl;

                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    var oldFileName = oldImageUrl.Split("/").LastOrDefault();
                    var filePath = Path.Combine("C:/inetpub/wwwroot/backend2/wwwroot/uploads/", oldFileName);

                    _fileManagerService.DeleteFile(filePath);
                }

                await _companyRepository.RemoveAsync(entity);

                TempData["SuccessMessage"] = "Company removed successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while removing the company: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Recover(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _companyRepository.GetByIdAsync(x => x.Id == id);
                await _companyRepository.RecoverAsync(entity);

                TempData["SuccessMessage"] = "Company recovered successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while recovering the company: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }
    }
}
