using System.Globalization;
using AppTech.Business.DTOs.GiftCardDTOs;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Enums;
using AppTech.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.MVC.Controllers
{
    public class GiftCardController : BaseController
    {
        private readonly IGiftCardService _giftCardService;
        private readonly IFileManagerService _fileManagerService;
        private readonly IGiftCardRepository _giftCardRepository;

        public GiftCardController(IGiftCardService giftCardService, IGiftCardRepository giftCardRepository, IFileManagerService fileManagerService)
        {
            _giftCardService = giftCardService;
            _giftCardRepository = giftCardRepository;
            _fileManagerService = fileManagerService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 8)
        {
            try
            {
                var query = await (User.IsInRole("Admin")
                    ? _giftCardRepository.GetAllAsync(x => true)
                    : _giftCardRepository.GetAllAsync(x => !x.IsDeleted));

                var count = query.Count();
                var selectedQuery = query
                    .OrderByDescending(t => t.UpdatedOn)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .AsQueryable();

                var totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var data = new PaginatedQueryable<GiftCard>(selectedQuery, pageIndex, totalPages);

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading gift cards: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateGiftCardDTO dto, string price)
        {
            var decimalBalance = Convert.ToDecimal(price, CultureInfo.InvariantCulture);
            dto.Price = decimalBalance;

            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Invalid data provided.";
                    return View(dto);
                }

                await _giftCardService.AddAsync(dto);
                TempData["SuccessMessage"] = "Gift card created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating gift card: \n{ex.Message}\n{ex.InnerException?.Message}";
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id, int pageIndex = 1)
        {
            try
            {
                var giftCard = await _giftCardService.GetByIdAsync(new GetByIdGiftCardDTO { Id = id });

                if (giftCard == null)
                {
                    TempData["ErrorMessage"] = "Gift card not found.";
                    return RedirectToAction("Index");
                }

                var type = giftCard.Type switch
                {
                    "standard" => EGiftCardType.Standard,
                    "premium" => EGiftCardType.Premium,
                    "vip" => EGiftCardType.VIP,
                    _ => throw new ArgumentOutOfRangeException(nameof(giftCard.Type), giftCard.Type, null)
                };

                var model = new UpdateGiftCardDTO
                {
                    Id = id,
                    Price = giftCard.Price,
                    Type = type,
                    DashImageUrl = giftCard.ImageUrl,
                    PageIndex = pageIndex
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading gift card: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateGiftCardDTO dto, string price)
        {
            var decimalBalance = Convert.ToDecimal(price, CultureInfo.InvariantCulture);
            dto.Price = decimalBalance;

            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Invalid data provided.";
                    return View(dto);
                }

                await _giftCardService.UpdateAsync(dto);
                TempData["SuccessMessage"] = "Gift card updated successfully.";
                return RedirectToAction("Index", new { pageIndex = dto.PageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating gift card: \n{ex.Message}\n{ex.InnerException?.Message}";
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, int pageIndex = 1)
        {
            try
            {
                await _giftCardService.DeleteAsync(new DeleteGiftCardDTO { Id = id });
                TempData["SuccessMessage"] = "Gift card deleted successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting gift card: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _giftCardRepository.GetByIdAsync(x => x.Id == id);

                if (entity == null)
                {
                    TempData["ErrorMessage"] = "Gift card not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                var oldImageUrl = entity.ImageUrl;

                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    var oldFileName = oldImageUrl.Split("/").LastOrDefault();
                    var filePath = Path.Combine("C:/inetpub/wwwroot/backend2/wwwroot/uploads/", oldFileName);

                    _fileManagerService.DeleteFile(filePath);
                }

                await _giftCardRepository.RemoveAsync(entity);
                TempData["SuccessMessage"] = "Gift card removed successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while removing gift card: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Recover(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _giftCardRepository.GetByIdAsync(x => x.Id == id);

                if (entity == null)
                {
                    TempData["ErrorMessage"] = "Gift card not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                await _giftCardRepository.RecoverAsync(entity);
                TempData["SuccessMessage"] = "Gift card recovered successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while recovering gift card: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }
    }
}
