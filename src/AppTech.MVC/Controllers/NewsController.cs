using AppTech.Business.DTOs.NewsDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.MVC.Controllers
{
    public class NewsController : BaseController
    {
        private readonly INewsTranslationRepository _newsTranslationRepository;
        private readonly INewsTranslationService _newsTranslationService;
        private readonly IFileManagerService _fileManagerService;
        private readonly INewsRepository _newsRepository;
        private readonly INewsService _newsService;
        private readonly IMapper _mapper;

        public NewsController(INewsService newsService, IMapper mapper, INewsTranslationService newsTranslationService, INewsRepository newsRepository, INewsTranslationRepository newsTranslationRepository, IFileManagerService fileManagerService)
        {
            _newsService = newsService;
            _mapper = mapper;
            _newsTranslationService = newsTranslationService;
            _newsRepository = newsRepository;
            _newsTranslationRepository = newsTranslationRepository;
            _fileManagerService = fileManagerService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 8)
        {
            try
            {
                var query = await (User.IsInRole("Admin")
                    ? _newsRepository.GetAllAsync(x => true)
                    : _newsRepository.GetAllAsync(x => !x.IsDeleted));

                var count = query.Count();
                var selectedQuery = query
                    .OrderByDescending(t => t.UpdatedOn)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .AsQueryable();

                var totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var data = new PaginatedQueryable<News>(selectedQuery, pageIndex, totalPages);

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading news: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateNewsDTO dto)
        {
            try
            {
                var createdNews = await _newsService.AddAsync(dto);

                if (dto.Translations != null && dto.Translations.Any())
                {
                    foreach (var translation in dto.Translations)
                    {
                        if (!string.IsNullOrWhiteSpace(translation.Description))
                        {
                            translation.NewsId = createdNews.Id;
                            await _newsTranslationService.AddAsync(translation);
                        }
                    }
                }

                TempData["SuccessMessage"] = "News created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating news: \n{ex.Message}\n{ex.InnerException?.Message}";
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id, int pageIndex = 1)
        {
            try
            {
                var news = await _newsService.GetByIdAsync(new GetByIdNewsDTO() { Id = id });

                if (news == null)
                {
                    TempData["ErrorMessage"] = "News not found.";
                    return RedirectToAction("Index");
                }

                var translations = news.Translations.Select(t => new UpdateNewsTranslationDTO
                {
                    Id = t?.Id ?? 0,
                    NewsId = news.Id,
                    Description = t?.Description ?? string.Empty,
                    Language = LanguageChanger.Change(t.Language)
                }).ToList();

                var model = new UpdateNewsDTO
                {
                    Id = news.Id,
                    Title = news.Title,
                    Url = news.Url,
                    Translations = translations,
                    DashImageUrl = news.ImageUrl,
                    PageIndex = pageIndex
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the news update page: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateNewsDTO dto, string removedTranslationIds)
        {
            try
            {
                var updatedNews = await _newsService.UpdateAsync(dto);

                if (!string.IsNullOrEmpty(removedTranslationIds))
                {
                    var idsToRemove = removedTranslationIds.Split(',').Select(int.Parse).ToList();
                    foreach (var id in idsToRemove)
                    {
                        await _newsTranslationRepository.RemoveAsync(
                            await _newsTranslationRepository.GetByIdAsync(x => x.Id == id));
                    }
                }

                if (dto.Translations is not null)
                {
                    foreach (var translation in dto.Translations)
                    {
                        if (translation.Description == null) continue;

                        if (translation.Id == 0)
                        {
                            var createTranslationDTO = new CreateNewsTranslationDTO
                            {
                                NewsId = updatedNews.Id,
                                Description = translation.Description,
                                Language = translation.Language
                            };
                            await _newsTranslationService.AddAsync(createTranslationDTO);
                        }
                        else
                        {
                            var updateTranslationDTO = new UpdateNewsTranslationDTO
                            {
                                Id = translation.Id,
                                NewsId = updatedNews.Id,
                                Description = translation.Description,
                                Language = translation.Language
                            };
                            await _newsTranslationService.UpdateAsync(updateTranslationDTO);
                        }
                    }
                }

                TempData["SuccessMessage"] = "News updated successfully.";
                return RedirectToAction("Index", new { pageIndex = dto.PageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the news: \n{ex.Message}\n{ex.InnerException?.Message}";
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, int pageIndex = 1)
        {
            try
            {
                await _newsService.DeleteAsync(new DeleteNewsDTO { Id = id });

                TempData["SuccessMessage"] = "News deleted successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the news: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _newsRepository.GetByIdAsync(x => x.Id == id);

                var oldImageUrl = entity.ImageUrl;

                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    var oldFileName = oldImageUrl.Split("/").LastOrDefault();
                    var filePath = Path.Combine("C:/inetpub/wwwroot/backend2/wwwroot/uploads/", oldFileName);

                    _fileManagerService.DeleteFile(filePath);
                }

                await _newsRepository.RemoveAsync(entity);

                TempData["SuccessMessage"] = "News removed successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while removing the news: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Recover(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _newsRepository.GetByIdAsync(x => x.Id == id);
                await _newsRepository.RecoverAsync(entity);

                TempData["SuccessMessage"] = "News recovered successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while recovering the news: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }
    }
}
