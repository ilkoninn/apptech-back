using AppTech.Business.DTOs.ContactUsDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.MVC.Controllers
{
    public class ContactUsController : BaseController
    {
        private readonly IContactUsService _contactUsService;
        private readonly IContactUsRepository _contactUsRepository;

        public ContactUsController(IContactUsService contactUsService, IContactUsRepository contactUsRepository)
        {
            _contactUsService = contactUsService;
            _contactUsRepository = contactUsRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 8)
        {
            try
            {
                var query = await (User.IsInRole("Admin")
                    ? _contactUsRepository.GetAllAsync(x => true)
                    : _contactUsRepository.GetAllAsync(x => !x.IsDeleted));

                var count = query.Count();
                var selectedQuery = query
                    .OrderByDescending(t => t.UpdatedOn)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .AsQueryable();

                var totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var data = new PaginatedQueryable<ContactUs>(selectedQuery, pageIndex, totalPages);

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the contact messages: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id, int pageIndex = 1)
        {
            try
            {
                var contactUs = await _contactUsService.GetByIdAsync(new GetByIdContactUsDTO() { Id = id });

                if (contactUs == null)
                {
                    TempData["ErrorMessage"] = "Contact message not found.";
                    return RedirectToAction("Index");
                }

                return View(new ContactUsDTO
                {
                    FullName = contactUs.FullName,
                    Message = contactUs.Message,
                    ImageUrl = contactUs.ImageUrl,
                    Email = contactUs.Email,
                    Subject = contactUs.Subject,
                    PageIndex = pageIndex,
                });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the contact message details: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, int pageIndex = 1)
        {
            try
            {
                await _contactUsService.DeleteAsync(new DeleteContactUsDTO() { Id = id });
                TempData["SuccessMessage"] = "Contact message deleted successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the contact message: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _contactUsRepository.GetByIdAsync(x => x.Id == id);

                if (entity == null)
                {
                    TempData["ErrorMessage"] = "Contact message not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                await _contactUsRepository.RemoveAsync(entity);
                TempData["SuccessMessage"] = "Contact message removed successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while removing the contact message: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Recover(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _contactUsRepository.GetByIdAsync(x => x.Id == id);

                if (entity == null)
                {
                    TempData["ErrorMessage"] = "Contact message not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                await _contactUsRepository.RecoverAsync(entity);
                TempData["SuccessMessage"] = "Contact message recovered successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while recovering the contact message: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }
    }
}
