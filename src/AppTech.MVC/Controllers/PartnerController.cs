using AppTech.Business.DTOs.PartnerDTOs;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Business.Services.InternalServices.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PartnerController : Controller
    {
        private readonly IPartnerRepository _partnerRepository;
        private readonly IFileManagerService _fileManagerService;
        private readonly IPartnerService _partnerService;
        private readonly IMapper _mapper;

        public PartnerController(IPartnerService partnerService, IPartnerRepository partnerRepository, IMapper mapper, IFileManagerService fileManagerService)
        {
            _fileManagerService = fileManagerService;
            _partnerRepository = partnerRepository;
            _partnerService = partnerService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 8, bool? isAccepted = null)
        {
            try
            {
                var query = User.IsInRole("Admin")
                    ? await _partnerRepository.GetAllAsync(x => isAccepted == null || x.IsAccepted == isAccepted.Value)
                    : await _partnerRepository.GetAllAsync(x => !x.IsDeleted && (isAccepted == null || x.IsAccepted == isAccepted.Value));

                var count = query.Count();
                var selectedQuery = query
                    .OrderByDescending(t => t.UpdatedOn)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .AsQueryable();

                var totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var data = new PaginatedQueryable<Partner>(selectedQuery, pageIndex, totalPages);

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading partners: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id, int pageIndex = 1)
        {
            try
            {
                var partner = await _partnerRepository.GetByIdAsync(x => x.Id == id);

                if (partner == null)
                {
                    TempData["ErrorMessage"] = "Partner not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                var model = new UpdatePartnerDTO
                {
                    Id = partner.Id,
                    Company = partner.Company,
                    Url = partner.Url,
                    DashImageUrl = partner.ImageUrl,
                    PageIndex = pageIndex
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the partner: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdatePartnerDTO dto)
        {
            try
            {
                await _partnerService.UpdateAsync(dto);
                TempData["SuccessMessage"] = "Partner updated successfully.";
                return RedirectToAction("Index", new { pageIndex = dto.PageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the partner: \n{ex.Message}\n{ex.InnerException?.Message}";
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, int pageIndex = 1)
        {
            try
            {
                await _partnerService.DeleteAsync(new DeletePartnerDTO() { Id = id });
                TempData["SuccessMessage"] = "Partner deleted successfully.";
                var redirectUrl = pageIndex != 0 ? $"/Partner?pageIndex={pageIndex}" : "/Partner";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the partner: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _partnerRepository.GetByIdAsync(x => x.Id == id);

                var oldImageUrl = entity.ImageUrl;

                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    var oldFileName = oldImageUrl.Split("/").LastOrDefault();
                    var filePath = Path.Combine("C:/inetpub/wwwroot/backend2/wwwroot/uploads/", oldFileName);

                    _fileManagerService.DeleteFile(filePath);
                }

                await _partnerRepository.RemoveAsync(entity);

                TempData["SuccessMessage"] = "Partner removed successfully.";
                var redirectUrl = pageIndex != 0 ? $"/Partner?pageIndex={pageIndex}" : "/Partner";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while removing the partner: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ToggleAccept(int id, int pageIndex = 1)
        {
            try
            {
                var partner = await _partnerRepository.GetByIdAsync(x => x.Id == id);

                if (partner == null)
                {
                    TempData["ErrorMessage"] = "Partner not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                partner.IsAccepted = !partner.IsAccepted;
                await _partnerRepository.UpdateAsync(partner);

                TempData["SuccessMessage"] = $"Partner acceptance status updated successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating partner acceptance status: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Recover(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _partnerRepository.GetByIdAsync(x => x.Id == id);
                await _partnerRepository.RecoverAsync(entity);

                TempData["SuccessMessage"] = "Partner recovered successfully.";
                var redirectUrl = pageIndex != 0 ? $"/Partner?pageIndex={pageIndex}" : "/Partner";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while recovering the partner: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }
    }
}
