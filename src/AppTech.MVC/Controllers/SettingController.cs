using AppTech.Business.DTOs.SettingDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.InternalServices.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Enums;
using AppTech.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppTech.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingController : Controller
    {
        private readonly ISettingTranslationRepository _settingTranslationRepository;
        private readonly ISettingTranslationService _settingTranslationService;
        private readonly ISettingRepository _settingRepository;
        private readonly ISettingService _settingService;

        public SettingController(
            ISettingService settingService,
            ISettingTranslationService settingTranslationService,
            ISettingRepository settingRepository,
            ISettingTranslationRepository settingTranslationRepository)
        {
            _settingTranslationService = settingTranslationService;
            _settingRepository = settingRepository;
            _settingService = settingService;
            _settingTranslationRepository = settingTranslationRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 8)
        {
            try
            {
                var isAdmin = User.IsInRole("Admin");
                var settings = await _settingRepository.GetAllAsync(s => isAdmin ? true : !s.IsDeleted);

                var count = settings.Count;

                var selectedSettings = settings
                    .OrderByDescending(t => t.UpdatedOn)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var data = new PaginatedQueryable<Setting>(selectedSettings.AsQueryable(), pageIndex, totalPages);

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading settings: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSettingDTO dto)
        {
            try
            {
                var createdSetting = await _settingService.AddAsync(dto);

                if (dto.Translations != null && dto.Translations.Any())
                {
                    foreach (var translation in dto.Translations)
                    {
                        if (translation.Value == null) continue;

                        translation.SettingId = createdSetting.Id.GetValueOrDefault();
                        await _settingTranslationService.AddAsync(translation);
                    }
                }

                TempData["SuccessMessage"] = "Setting created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the setting: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddEditTOS()
        {
            var settings = await _settingRepository.GetAllAsync(x => x.IsDeleted);

            var setting = settings.FirstOrDefault(x => x.Key == "TOS");

            if (setting is not null)
                return View(setting);

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddEditTOS(CreateSettingDTO dto)
        {
            try
            {
                var createdSetting = await _settingService.AddAsync(dto);

                dto.Translations.First().Language = ELanguage.En;

                if (dto.Translations != null && dto.Translations.Any())
                {
                    foreach (var translation in dto.Translations)
                    {
                        if (translation.Value == null) continue;

                        translation.SettingId = createdSetting.Id.GetValueOrDefault();
                        await _settingTranslationService.AddAsync(translation);
                    }
                }

                TempData["SuccessMessage"] = "Setting created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the setting: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id, int pageIndex = 1)
        {
            try
            {
                var setting = await _settingService.GetByIdAsync(new GetByIdSettingDTO { Id = id });

                if (setting == null)
                {
                    TempData["ErrorMessage"] = "Setting not found.";
                    return RedirectToAction("Index");
                }

                var translations = new List<UpdateSettingTranslationDTO>();

                foreach (var translation in setting.Translations)
                {
                    translations.Add(new UpdateSettingTranslationDTO
                    {
                        Id = translation.Id,
                        SettingId = translation.SettingId,
                        Value = translation?.Value ?? string.Empty,
                        Language = LanguageChanger.Change(translation.Language)
                    });
                }

                var model = new UpdateSettingDTO
                {
                    Id = id,
                    Key = setting.Key,
                    Type = setting.Type,
                    Page = setting.Page,
                    Translations = translations,
                    PageIndex = pageIndex
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the setting for update: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateSettingDTO dto, string removedTranslationIds)
        {
            try
            {
                var updatedSetting = await _settingService.UpdateAsync(dto);

                if (!string.IsNullOrEmpty(removedTranslationIds))
                {
                    var idsToRemove = removedTranslationIds.Split(',').Select(int.Parse).ToList();

                    foreach (var id in idsToRemove)
                    {
                        await _settingTranslationRepository.RemoveAsync(
                            await _settingTranslationRepository.GetByIdAsync(x => x.Id == id));
                    }
                }

                if (dto.Translations is not null)
                {
                    foreach (var translation in dto.Translations)
                    {
                        if (translation.Id == 0)
                        {
                            var createTranslationDTO = new CreateSettingTranslationDTO
                            {
                                SettingId = (int)updatedSetting.Id,
                                Value = translation.Value,
                                Language = translation.Language
                            };

                            await _settingTranslationService.AddAsync(createTranslationDTO);
                        }
                        else
                        {
                            var updateTranslationDTO = new UpdateSettingTranslationDTO
                            {
                                Id = translation.Id,
                                SettingId = (int)updatedSetting.Id,
                                Value = translation.Value,
                                Language = translation.Language
                            };

                            await _settingTranslationService.UpdateAsync(updateTranslationDTO);
                        }
                    }
                }

                TempData["SuccessMessage"] = "Setting updated successfully.";
                return RedirectToAction("Index", new { pageIndex = dto.PageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the setting: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex = dto.PageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, int pageIndex = 1)
        {
            try
            {
                await _settingService.DeleteAsync(new DeleteSettingDTO { Id = id });

                TempData["SuccessMessage"] = "Setting deleted successfully.";
                var redirectUrl = pageIndex != 0 ? $"/Setting?pageIndex={pageIndex}" : "/Setting";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the setting: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _settingRepository.GetByIdAsync(x => x.Id == id);

                await _settingRepository.RemoveAsync(entity);

                TempData["SuccessMessage"] = "Setting removed successfully.";
                var redirectUrl = pageIndex != 0 ? $"/Setting?pageIndex={pageIndex}" : "/Setting";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while removing the setting: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Recover(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _settingRepository.GetByIdAsync(x => x.Id == id);
                await _settingRepository.RecoverAsync(entity);

                TempData["SuccessMessage"] = "Setting recovered successfully.";
                var redirectUrl = pageIndex != 0 ? $"/Setting?pageIndex={pageIndex}" : "/Setting";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while recovering the setting: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }
    }
}
