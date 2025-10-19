using AppTech.Business.DTOs.CertificationDTOs;
using AppTech.Business.DTOs.PromotionDTOs;
using AppTech.Business.MappingProfiles;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppTech.MVC.Controllers
{
    public class PromotionController : BaseController
    {
        private readonly IPromotionService _promotionService;
        private readonly IPromotionRepository _promotionRepository;
        private readonly ICertificationService _certificationService;

        public PromotionController(
            IPromotionService promotionService,
            IPromotionRepository promotionRepository,
            ICertificationService certificationService)
        {
            _promotionService = promotionService;
            _promotionRepository = promotionRepository;
            _certificationService = certificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 8)
        {
            try
            {
                var query = User.IsInRole("Admin")
                    ? await _promotionRepository.GetAllAsync(x => true)
                    : await _promotionRepository.GetAllAsync(x => !x.IsDeleted);

                var count = query.Count();
                var selectedQuery = query
                    .OrderByDescending(t => t.UpdatedOn)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .AsQueryable();

                var totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var data = new PaginatedQueryable<Promotion>(selectedQuery, pageIndex, totalPages);

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading promotions: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var certifications = await _certificationService.GetAllAsync(new GetAllCertificationDTO());

            try
            {
                ViewBag.Certifications = new SelectList(certifications, "Id", "Title");

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading certifications: \n{ex.Message}\n{ex.InnerException?.Message}";
                ViewBag.Certifications = new SelectList(certifications, "Id", "Title");

                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePromotionDTO dto)
        {
            var certifications = await _certificationService.GetAllAsync(new GetAllCertificationDTO());

            try
            {
                if (dto.CertificationIds == null || !dto.CertificationIds.Any())
                {
                    ModelState.AddModelError("", "Please select certifications.");
                    return View(dto);
                }

                await _promotionService.AddAsync(dto);
                TempData["SuccessMessage"] = "Promotion created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the promotion: \n{ex.Message}\n{ex.InnerException?.Message}";
                ViewBag.Certifications = new SelectList(certifications, "Id", "Title");

                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id, int pageIndex = 1)
        {
            var certifications = await _certificationService.GetAllAsync(new GetAllCertificationDTO());

            try
            {
                var promotion = await _promotionRepository.GetByIdAsync(x => x.Id == id, x => x.CertificationPromotions);

                if (promotion == null)
                {
                    TempData["ErrorMessage"] = "Promotion not found.";
                    return RedirectToAction("Index");
                }

                var model = new UpdatePromotionDTO
                {
                    Id = promotion.Id,
                    Code = promotion.Code,
                    Percentage = promotion.Percentage,
                    EndedOn = promotion.EndedOn,
                    CertificationIds = promotion.CertificationPromotions.Select(e => e.CertificationId).ToList(),
                    PageIndex = pageIndex
                };

                ViewBag.Certifications = new SelectList(certifications, "Id", "Title", model.CertificationIds);

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the promotion: \n{ex.Message}\n{ex.InnerException?.Message}";
                ViewBag.Certifications = new SelectList(certifications, "Id", "Title");

                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdatePromotionDTO dto)
        {
            var certifications = await _certificationService.GetAllAsync(new GetAllCertificationDTO());

            try
            {
                if (dto.CertificationIds == null || !dto.CertificationIds.Any())
                {
                    ModelState.AddModelError("", "Please select certifications.");
                    return View(dto);
                }

                await _promotionService.UpdateAsync(dto);
                TempData["SuccessMessage"] = "Promotion updated successfully.";
                return RedirectToAction("Index", new { pageIndex = dto.PageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the promotion: \n{ex.Message}\n{ex.InnerException?.Message}";
                ViewBag.Certifications = new SelectList(certifications, "Id", "Title");

                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, int pageIndex = 1)
        {
            try
            {
                await _promotionService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Promotion deleted successfully.";
                var redirectUrl = pageIndex != 0 ? $"/Promotion?pageIndex={pageIndex}" : "/Promotion";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the promotion: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _promotionRepository.GetByIdAsync(x => x.Id == id);
                await _promotionRepository.RemoveAsync(entity);

                TempData["SuccessMessage"] = "Promotion removed successfully.";
                var redirectUrl = pageIndex != 0 ? $"/Promotion?pageIndex={pageIndex}" : "/Promotion";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while removing the promotion: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Recover(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _promotionRepository.GetByIdAsync(x => x.Id == id);
                await _promotionRepository.RecoverAsync(entity);

                TempData["SuccessMessage"] = "Promotion recovered successfully.";
                var redirectUrl = pageIndex != 0 ? $"/Promotion?pageIndex={pageIndex}" : "/Promotion";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while recovering the promotion: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }
    }
}
