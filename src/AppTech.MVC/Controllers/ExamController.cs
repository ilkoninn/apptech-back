using System.Globalization;
using AppTech.Business.DTOs.CertificationDTOs;
using AppTech.Business.DTOs.ExamDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Entities.Identity;
using AppTech.DAL.Repositories.Abstractions;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.MVC.Controllers
{
    public class ExamController : BaseController
    {
        private readonly ISubscriptionUserRepository _subscriptionUserRepository;
        private readonly IExamTranslationRepository _examTranslationRepository;
        private readonly ICertificationRepository _certificationRepository;
        private readonly IExamTranslationService _examTranslationService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICertificationService _certificationService;
        private readonly IExamResultRepository _examResultRepository;
        private readonly IExamRepository _examRepository;
        private readonly IExamService _examService;
        private readonly IMapper _mapper;

        public ExamController(IExamService examService, IMapper mapper, IExamTranslationService examTranslationService, ICertificationService certificationService, IExamRepository examRepository, IExamTranslationRepository examTranslationRepository, ICertificationRepository certificationRepository, ISubscriptionUserRepository subscriptionUserRepository, IExamResultRepository examResultRepository, ITransactionRepository transactionRepository)
        {
            _mapper = mapper;
            _examService = examService;
            _examRepository = examRepository;
            _certificationService = certificationService;
            _examResultRepository = examResultRepository;
            _transactionRepository = transactionRepository;
            _examTranslationService = examTranslationService;
            _certificationRepository = certificationRepository;
            _examTranslationRepository = examTranslationRepository;
            _subscriptionUserRepository = subscriptionUserRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 8)
        {
            try
            {
                var query = await (User.IsInRole("Admin")
                    ? _examRepository.GetAllAsync(x => true)
                    : _examRepository.GetAllAsync(x => !x.IsDeleted));

                var count = query.Count();
                var selectedQuery = query
                    .OrderByDescending(t => t.UpdatedOn)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .AsQueryable();

                var totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var data = new PaginatedQueryable<Exam>(selectedQuery, pageIndex, totalPages);

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading exams: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var certifications = await _certificationRepository.GetAllAsync(x => !x.IsDeleted);
                ViewBag.Certifications = certifications;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while preparing the creation form: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateExamDTO dto, string price)
        {
            var certifications = await _certificationRepository.GetAllAsync(x => !x.IsDeleted);
            var decimalBalance = Convert.ToDecimal(price, CultureInfo.InvariantCulture);
            dto.Price = decimalBalance;

            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Certifications = certifications;
                    TempData["ErrorMessage"] = "Invalid data provided.";
                    return View(dto);
                }

                var createdExam = await _examService.AddAsync(dto);

                if (dto.Translations != null && dto.Translations.Any())
                {
                    foreach (var translation in dto.Translations)
                    {
                        if (translation.Description == null) continue;

                        translation.ExamId = createdExam.Id;
                        await _examTranslationService.AddAsync(translation);
                    }
                }

                var usersWithActiveSubscriptions = await _subscriptionUserRepository
                    .GetAllAsync(x => !x.IsDeleted && x.Subscription.CertificationId == dto.CertificationId && x.ExpiredOn > DateTime.UtcNow,
                    s => s.Subscription,
                    u => u.User);

                foreach (var subscriptionUser in usersWithActiveSubscriptions)
                {
                    var hasExamResult = (await _examResultRepository
                        .GetAllAsync(x => !x.IsDeleted))
                        .Any(x => x.UserId == subscriptionUser.UserId && x.ExamId == createdExam.Id);

                    if (!hasExamResult)
                    {
                        var newExamResult = new ExamResult
                        {
                            UserId = subscriptionUser.UserId,
                            ExamId = createdExam.Id,
                        };

                        await _examResultRepository.AddAsync(newExamResult);
                    }
                }


                TempData["SuccessMessage"] = "Exam created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the exam: \n{ex.Message}\n{ex.InnerException?.Message}";
                ViewBag.Certifications = certifications;

                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id, int pageIndex = 1)
        {
            try
            {
                var exam = await _examService.GetByIdAsync(new GetByIdExamDTO() { Id = id });

                if (exam == null)
                {
                    TempData["ErrorMessage"] = "Exam not found.";
                    return RedirectToAction("Index");
                }

                var translations = new List<UpdateExamTranslationDTO>();

                foreach (var translation in exam.Translations)
                {
                    translations.Add(new UpdateExamTranslationDTO
                    {
                        Id = translation.Id,
                        ExamId = translation.ExamId,
                        Description = translation?.Description ?? string.Empty,
                        Language = LanguageChanger.Change(translation.Language)
                    });
                }

                var certifications = await _certificationRepository.GetAllAsync(x => !x.IsDeleted);
                ViewBag.Certifications = certifications;

                var model = new UpdateExamDTO()
                {
                    Id = exam.Id,
                    CertificationId = exam.CertificationId,
                    Code = exam.Code,
                    MaxScore = exam.MaxScore,
                    PassScore = exam.PassScore,
                    Duration = exam.Duration,
                    Price = exam.Price,
                    QuestionCount = exam.QuestionCount,
                    Translations = translations,
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
        public async Task<IActionResult> Update(UpdateExamDTO dto, string removedTranslationIds, string price)
        {
            var certifications = await _certificationService.GetAllAsync(new GetAllCertificationDTO());
            var decimalBalance = Convert.ToDecimal(price, CultureInfo.InvariantCulture);
            dto.Price = decimalBalance;

            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Invalid data provided.";
                    return View(dto);
                }

                var updatedExam = await _examService.UpdateAsync(dto);

                if (!string.IsNullOrEmpty(removedTranslationIds))
                {
                    var idsToRemove = removedTranslationIds.Split(',').Select(int.Parse).ToList();

                    foreach (var id in idsToRemove)
                    {
                        await _examTranslationRepository.RemoveAsync(
                            await _examTranslationRepository.GetByIdAsync(x => x.Id == id));
                    }
                }

                if (dto.Translations is not null)
                {
                    foreach (var translation in dto.Translations)
                    {
                        if (translation.Description == null) continue;

                        if (translation.Id == 0)
                        {
                            // Add new translation
                            var createTranslationDTO = new CreateExamTranslationDTO
                            {
                                ExamId = updatedExam.Id,
                                Description = translation.Description,
                                Language = translation.Language
                            };

                            await _examTranslationService.AddAsync(createTranslationDTO);
                        }
                        else
                        {
                            // Update existing translation
                            var updateTranslationDTO = new UpdateExamTranslationDTO
                            {
                                Id = translation.Id,
                                ExamId = updatedExam.Id,
                                Description = translation.Description,
                                Language = translation.Language
                            };

                            await _examTranslationService.UpdateAsync(updateTranslationDTO);
                        }
                    }
                }

                TempData["SuccessMessage"] = "Exam updated successfully.";
                return RedirectToAction("Index", new { pageIndex = dto.PageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the exam: \n{ex.Message}\n{ex.InnerException?.Message}";
                ViewBag.Certifications = certifications;

                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, int pageIndex = 1)
        {
            try
            {
                await _examService.DeleteAsync(new DeleteExamDTO() { Id = id });
                TempData["SuccessMessage"] = "Exam deleted successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the exam: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _examRepository.GetByIdAsync(x => x.Id == id);

                if (entity == null)
                {
                    TempData["ErrorMessage"] = "Exam not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                await _examRepository.RemoveAsync(entity);

                TempData["SuccessMessage"] = "Exam removed successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while removing the exam: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Recover(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _examRepository.GetByIdAsync(x => x.Id == id);

                if (entity == null)
                {
                    TempData["ErrorMessage"] = "Exam not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                await _examRepository.RecoverAsync(entity);
                TempData["SuccessMessage"] = "Exam recovered successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while recovering the exam: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }
    }
}
