using AppTech.Business.DTOs.FAQDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.MVC.Controllers
{
    public class FAQController : BaseController
    {
        private readonly IFAQService _faqService;
        private readonly IFAQRepository _faqRepository;

        public FAQController(IFAQService faqService, IFAQRepository faqRepository)
        {
            _faqService = faqService;
            _faqRepository = faqRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 8)
        {
            try
            {
                var query = await (User.IsInRole("Admin")
                    ? _faqRepository.GetAllAsync(x => true)
                    : _faqRepository.GetAllAsync(x => !x.IsDeleted));

                var count = query.Count();
                var selectedQuery = query
                    .OrderByDescending(t => t.UpdatedOn)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .AsQueryable();

                var totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var data = new PaginatedQueryable<FAQ>(selectedQuery, pageIndex, totalPages);

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading FAQs: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateFAQDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Invalid data provided.";
                    return View(dto);
                }

                await _faqService.AddAsync(dto);
                TempData["SuccessMessage"] = "FAQ created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the FAQ: \n{ex.Message}\n{ex.InnerException?.Message}";
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id, int pageIndex = 1)
        {
            try
            {
                var faq = await _faqService.GetByIdAsync(new GetByIdFAQDTO { Id = id });

                if (faq == null)
                {
                    TempData["ErrorMessage"] = "FAQ not found.";
                    return RedirectToAction("Index");
                }

                var model = new UpdateFAQDTO
                {
                    Id = id,
                    Answer = faq.Answer,
                    Question = faq.Question,
                    Language = LanguageChanger.Change(faq.Language),
                    PageIndex = pageIndex
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the FAQ: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateFAQDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Invalid data provided.";
                    return View(dto);
                }

                await _faqService.UpdateAsync(dto);
                TempData["SuccessMessage"] = "FAQ updated successfully.";
                return RedirectToAction("Index", new { pageIndex = dto.PageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the FAQ: \n{ex.Message}\n{ex.InnerException?.Message}";
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, int pageIndex = 1)
        {
            try
            {
                await _faqService.DeleteAsync(new DeleteFAQDTO { Id = id });
                TempData["SuccessMessage"] = "FAQ deleted successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the FAQ: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _faqRepository.GetByIdAsync(x => x.Id == id);

                if (entity == null)
                {
                    TempData["ErrorMessage"] = "FAQ not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                await _faqRepository.RemoveAsync(entity);
                TempData["SuccessMessage"] = "FAQ removed successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while removing the FAQ: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Recover(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _faqRepository.GetByIdAsync(x => x.Id == id);

                if (entity == null)
                {
                    TempData["ErrorMessage"] = "FAQ not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                await _faqRepository.RecoverAsync(entity);
                TempData["SuccessMessage"] = "FAQ recovered successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while recovering the FAQ: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }
    }
}
