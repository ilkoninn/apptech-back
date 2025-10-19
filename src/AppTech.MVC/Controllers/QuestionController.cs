using System.Globalization;
using System.Text.RegularExpressions;
using AppTech.Business.DTOs.CertificationDTOs;
using AppTech.Business.DTOs.QuestionDTOs;
using AppTech.Business.DTOs.VariantDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.MappingProfiles;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Business.Services.Interfaces;
using AppTech.Business.Services.InternalServices.Abstractions;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Enums;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Abstractions;
using AppTech.DAL.Repositories.Interfaces;
using AppTech.MVC.ViewModels.QuestionVMs;
using AppTech.MVC.ViewModels.UserVMs;
using AutoMapper;
using Humanizer;
using iText.Layout.Element;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static iText.IO.Util.IntHashtable;

namespace AppTech.MVC.Controllers
{
    public class QuestionController : BaseController
    {
        private readonly IDropZoneDragVariantRepository _dropZoneDragVariantRepository;
        private readonly IDragDropQuestionRepository _dragDropQuestionRepository;
        private readonly ICertificationRepository _certificationRepository;
        private readonly IQuestionImageRepository _questionImageRepository;
        private readonly IDragVariantRepository _dragVariantRepository;
        private readonly ICertificationService _certificationService;
        private readonly IFileManagerService _fileManagerService;
        private readonly IDropZoneRepository _dropZoneRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IVariantRepository _variantRepository;
        private readonly IQuestionService _questionService;
        private readonly IVariantService _variantService;
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;

        public QuestionController(IQuestionService questionService, IVariantService variantService,
            IMapper mapper, ICertificationService certificationService, AppDbContext appDbContext,
            IQuestionRepository questionRepository, IVariantRepository variantRepository,
            IQuestionImageRepository questionImageRepository, IFileManagerService fileManagerService, IDropZoneDragVariantRepository dropZoneDragVariantRepository, IDropZoneRepository dropZoneRepository, IDragVariantRepository dragVariantRepository, ICertificationRepository certificationRepository, IDragDropQuestionRepository dropQuestionRepository)
        {
            _questionImageRepository = questionImageRepository;
            _certificationService = certificationService;
            _questionRepository = questionRepository;
            _variantRepository = variantRepository;
            _questionService = questionService;
            _variantService = variantService;
            _appDbContext = appDbContext;
            _mapper = mapper;
            _fileManagerService = fileManagerService;
            _dropZoneDragVariantRepository = dropZoneDragVariantRepository;
            _dropZoneRepository = dropZoneRepository;
            _dragVariantRepository = dragVariantRepository;
            _certificationRepository = certificationRepository;
            _dragDropQuestionRepository = dropQuestionRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm = "", string status = "", int pageIndex = 1, int pageSize = 8, int? certification = null)
        {
            try
            {
                TempData["CurrentSearchTerm"] = searchTerm;
                TempData["CurrentStatus"] = status;
                TempData["CurrentCertification"] = certification;

                ViewBag.Certifications = await _certificationRepository.GetAllAsync(x => !x.IsDeleted);

                var query = User.IsInRole("Admin") ?
                    await _questionRepository.GetAllAsync(x => true, qi => qi.QuestionImages, dr => dr.DragDropQuestion)
                    : await _questionRepository.GetAllAsync(x => !x.IsDeleted, qi => qi.QuestionImages, dr => dr.DragDropQuestion);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(q =>
                        q.Content.ToLower().Contains(searchTerm.ToLower()) ||
                        q.Point.ToString().ToLower().Contains(searchTerm.ToLower())).ToList();
                }

                if (!string.IsNullOrEmpty(status))
                {
                    if (status == "Report")
                    {
                        query = query.Where(q => q.IsReported).ToList();
                    }
                    else
                    {
                        if (Enum.TryParse<EQuestionType>(status, out var parsedStatus))
                        {
                            query = query.Where(q => q.Type == parsedStatus).ToList();
                        }
                    }
                }

                if (certification.HasValue && certification.Value > 0)
                {
                    query = query.Where(q => q.CertificationId == certification.Value).ToList();
                }

                var totalCount = query.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var paginatedQuestions = query
                    .OrderByDescending(q => q.UpdatedOn)
                    .OrderByDescending(q => q.IsReported)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var model = new PaginatedQueryable<Question>(paginatedQuestions.AsQueryable(), pageIndex, totalPages);

                ViewBag.PageSize = pageSize;

                TempData.Keep("CurrentSearchTerm");
                TempData.Keep("CurrentStatus");
                TempData.Keep("CurrentCertification");

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: \n{ex.Message}\n{ex.InnerException?.Message}";
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
                TempData["ErrorMessage"] = $"An error occurred while loading certifications: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateQuestionDTO createQuestionDTO)
        {
            var certifications = await _certificationRepository.GetAllAsync(x => !x.IsDeleted);

            try
            {
                var createdQuestion = await _questionService.AddAsync(createQuestionDTO);

                TempData["SuccessMessage"] = "Question created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the question: {ex.Message} \n{ex.InnerException?.Message}";
                ViewBag.Certifications = certifications;
                return View();
            }
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id, int pageIndex)
        {
            var certifications = await _certificationRepository.GetAllAsync(x => !x.IsDeleted);

            try
            {
                ViewBag.Certifications = certifications;

                var question = await _questionRepository.GetByIdAsync(x => x.Id == id, x => x.Variants, qi => qi.QuestionImages);
                if (question == null)
                {
                    TempData["ErrorMessage"] = "Question not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                var variants = question.Variants?.Select(v => new UpdateVariantDTO
                {
                    Id = v.Id,
                    Text = v.Text,
                    IsCorrect = v.IsCorrect,
                    QuestionId = v.QuestionId
                }).ToList();

                return View(new UpdateQuestionDTO
                {
                    CertificationId = question.CertificationId,
                    Id = question.Id,
                    Type = question.Type,
                    PageIndex = pageIndex,
                    Point = question.Point,
                    Server = question.Server,
                    Content = question.Content,
                    Variants = variants ?? new List<UpdateVariantDTO>(),
                    QuestionImages = question?.QuestionImages ?? new List<QuestionImage>(),
                });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the question for update: {ex.Message}";
                ViewBag.Certifications = certifications;
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateQuestionDTO dto, string removedVariantIds, string removedImageIds)
        {
            var certifications = await _certificationService.GetAllAsync(new GetAllCertificationDTO());

            try
            {
                var currentQuestion = await _questionRepository.GetByIdAsync(x => x.Id == dto.Id, x => x.Variants, x => x.QuestionImages);

                if (currentQuestion == null)
                {
                    TempData["ErrorMessage"] = "Question not found.";
                    return RedirectToAction("Index", new { pageIndex = dto.PageIndex });
                }

                currentQuestion.CertificationId = dto.CertificationId;
                currentQuestion.Content = dto.Content;
                currentQuestion.Server = dto.Server;
                currentQuestion.Point = dto.Point;
                currentQuestion.Type = dto.Type;

                if (!string.IsNullOrEmpty(removedVariantIds))
                {
                    var idsToRemove = removedVariantIds.Split(',').Select(int.Parse).ToList();
                    var variantsToRemove = currentQuestion.Variants.Where(v => idsToRemove.Contains(v.Id)).ToList();

                    foreach (var variant in variantsToRemove)
                    {
                        currentQuestion.Variants.Remove(variant);
                        await _variantRepository.RemoveAsync(variant);
                    }
                }

                if (!string.IsNullOrEmpty(removedImageIds))
                {
                    var idsToRemove = removedImageIds.Split(',').Select(int.Parse).ToList();
                    var imagesToRemove = currentQuestion.QuestionImages.Where(img => idsToRemove.Contains(img.Id)).ToList();

                    foreach (var image in imagesToRemove)
                    {
                        var oldImageUrl = image.ImageUrl;

                        if (!string.IsNullOrEmpty(oldImageUrl))
                        {
                            var oldFileName = oldImageUrl.Split("/").LastOrDefault();
                            var filePath = Path.Combine("C:/inetpub/wwwroot/backend2/wwwroot/uploads/", oldFileName);

                            _fileManagerService.DeleteFile(filePath);
                        }

                        currentQuestion.QuestionImages.Remove(image);
                        await _questionImageRepository.RemoveAsync(image);
                    }
                }

                if (dto.Variants is not null)
                {

                    foreach (var variantDto in dto.Variants)
                    {
                        if (variantDto.Id == 0)
                        {
                            var newVariant = new Variant
                            {
                                QuestionId = currentQuestion.Id,
                                Text = variantDto.Text,
                                IsCorrect = variantDto.IsCorrect
                            };
                            currentQuestion.Variants.Add(newVariant);
                            await _variantRepository.AddAsync(newVariant);
                        }
                        else
                        {
                            var existingVariant = currentQuestion.Variants.FirstOrDefault(v => v.Id == variantDto.Id);
                            if (existingVariant != null)
                            {
                                existingVariant.Text = variantDto.Text;
                                existingVariant.IsCorrect = variantDto.IsCorrect;
                                await _variantRepository.UpdateAsync(existingVariant);
                            }
                        }
                    }
                }

                if (dto.Images != null && dto.Images.Any())
                {
                    foreach (var image in dto.Images)
                    {
                        var imageUrl = await _fileManagerService.UploadFileAsync(image);

                        currentQuestion.QuestionImages.Add(new QuestionImage
                        {
                            QuestionId = currentQuestion.Id,
                            ImageUrl = imageUrl,
                        });
                    }
                }

                await _questionRepository.UpdateAsync(currentQuestion);

                TempData["SuccessMessage"] = "Question updated successfully.";
                return RedirectToAction("Index", new
                {
                    pageIndex = dto.PageIndex,
                    searchTerm = TempData.Peek("CurrentSearchTerm"),
                    status = TempData.Peek("CurrentStatus"),
                    certification = TempData.Peek("CurrentCertification")
                });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the question: {ex.Message}";
                ViewBag.Certifications = certifications;
                return RedirectToAction("Update", new { id = dto.Id, pageIndex = dto.PageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, int pageIndex = 1)
        {
            try
            {
                var question = await _questionRepository.GetByIdAsync(x => x.Id == id,
                    x => x.Variants,
                    c => c.Certification,
                    qi => qi.QuestionImages,
                    dr => dr.DragDropQuestion);

                var variants = question?.Variants?.Select(v => new VariantDTO
                {
                    Id = v.Id,
                    Text = v.Text,
                    IsCorrect = v.IsCorrect
                }).ToList() ?? null;

                if (question.Type == EQuestionType.DragAndDrop)
                {
                    var dropZoneDragVariants = await _dropZoneDragVariantRepository.GetAllAsync(
                        dzdv => dzdv.DragDropQuestionId == question.DragDropQuestion.Id,
                        dzdv => dzdv.DropZone,
                        dzdv => dzdv.DragVariant
                    );

                    var dragDropQuestionResponseDTO = new DragDropQuestionResponseDTO
                    {
                        Id = question.DragDropQuestion.Id,
                        ImageUrl = question.DragDropQuestion.ImageUrl,
                        dropZones = dropZoneDragVariants.Select(dzdv => new DropZoneResponseDTO
                        {
                            Id = dzdv.DropZone.Id,
                            X = dzdv.DropZone.X,
                            Y = dzdv.DropZone.Y,
                            Width = dzdv.DropZone.Width,
                            Height = dzdv.DropZone.Height
                        }).ToList(),
                        dragVariants = dropZoneDragVariants.Select(dzdv => new DragVariantResponseDTO
                        {
                            Id = dzdv.DragVariant.Id,
                            ImageUrl = dzdv.DragVariant.ImageUrl
                        }).ToList(),
                    };

                    var questionDto = new QuestionDTO
                    {
                        Id = question.Id,
                        PageIndex = pageIndex,
                        Point = question.Point,
                        Server = question.Server,
                        Content = question.Content,
                        Type = question.Type.ToString(),
                        DashVariants = variants ?? null,
                        IsReported = question.IsReported,
                        ReportFrom = question.ReportFrom,
                        Certification = question.Certification,
                        QuestionImages = question?.QuestionImages ?? new List<QuestionImage>(),
                        DragDropQuestion = dragDropQuestionResponseDTO
                    };

                    return View(questionDto);
                }
                else
                {
                    var questionDto = new QuestionDTO
                    {
                        Id = question.Id,
                        PageIndex = pageIndex,
                        Point = question.Point,
                        Server = question.Server,
                        Content = question.Content,
                        DashVariants = variants ?? null,
                        Type = question.Type.ToString(),
                        IsReported = question.IsReported,
                        ReportFrom = question.ReportFrom,
                        Certification = question.Certification,
                        QuestionImages = question?.QuestionImages ?? new List<QuestionImage>(),
                    };

                    return View(questionDto);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading question details: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, int pageIndex = 1)
        {
            try
            {
                await _questionService.DeleteAsync(new DeleteQuestionDTO { Id = id });

                TempData["SuccessMessage"] = "Question deleted successfully.";
                return RedirectToAction("Index", new
                {
                    pageIndex = pageIndex,
                    searchTerm = TempData.Peek("CurrentSearchTerm"),
                    status = TempData.Peek("CurrentStatus"),
                    certification = TempData.Peek("CurrentCertification")
                });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the question: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new
                {
                    pageIndex = pageIndex,
                    searchTerm = TempData.Peek("CurrentSearchTerm"),
                    status = TempData.Peek("CurrentStatus"),
                    certification = TempData.Peek("CurrentCertification")
                });
            }
        }

        public async Task<IActionResult> Remove(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _questionRepository.GetByIdAsync(x => x.Id == id,
                    d => d.DragDropQuestion);

                var imagesToRemove = (await _questionImageRepository
                    .GetAllAsync(x => !x.IsDeleted && x.QuestionId == entity.Id))
                    .ToList();

                var variantsToRemove = (await _variantRepository
                    .GetAllAsync(x => !x.IsDeleted && x.QuestionId == entity.Id))
                    .ToList();

                if(entity.DragDropQuestion is not null)
                {
                    var oldImageUrl = entity.DragDropQuestion.ImageUrl;

                    if (!string.IsNullOrEmpty(oldImageUrl))
                    {
                        var oldFileName = oldImageUrl.Split("/").LastOrDefault();
                        var filePath = Path.Combine("C:/inetpub/wwwroot/backend2/wwwroot/uploads/", oldFileName);

                        _fileManagerService.DeleteFile(filePath);
                    }

                    var dragVariantsAndDropZones = (await _dropZoneDragVariantRepository
                    .GetAllAsync(x => x.DragDropQuestionId == entity.DragDropQuestion.Id,
                    dz => dz.DropZone,
                    dv => dv.DragVariant))
                    .ToList();

                    if (dragVariantsAndDropZones.Any())
                    {
                        foreach (var dzv in dragVariantsAndDropZones)
                        {
                            await _dropZoneDragVariantRepository.RemoveAsync(dzv);

                            if (dzv.DropZone != null)
                            {
                                await _dropZoneRepository.RemoveAsync(dzv.DropZone);
                            }

                            if (dzv.DragVariant != null)
                            {
                                var oldVariantImageUrl = dzv.DragVariant.ImageUrl;

                                if (!string.IsNullOrEmpty(oldVariantImageUrl))
                                {
                                    var oldFileName = oldVariantImageUrl.Split("/").LastOrDefault();
                                    var filePath = Path.Combine("C:/inetpub/wwwroot/backend2/wwwroot/uploads/", oldFileName);

                                    _fileManagerService.DeleteFile(filePath);
                                }

                                await _dragVariantRepository.RemoveAsync(dzv.DragVariant);
                            }
                        }
                    }

                }

                if (imagesToRemove.Any())
                {
                    foreach (var image in imagesToRemove)
                    {
                        var oldImageUrl = image.ImageUrl;

                        if (!string.IsNullOrEmpty(oldImageUrl))
                        {
                            var oldFileName = oldImageUrl.Split("/").LastOrDefault();
                            var filePath = Path.Combine("C:/inetpub/wwwroot/backend2/wwwroot/uploads/", oldFileName);

                            _fileManagerService.DeleteFile(filePath);
                        }

                        await _questionImageRepository.RemoveAsync(image);
                    }
                }

                if (variantsToRemove.Any())
                {
                    foreach (var variant in variantsToRemove)
                    {
                        await _variantRepository.RemoveAsync(variant);
                    }
                }


                await _questionRepository.RemoveAsync(entity);

                TempData["SuccessMessage"] = "Question removed successfully.";
                return RedirectToAction("Index", new
                {
                    pageIndex = pageIndex,
                    searchTerm = TempData.Peek("CurrentSearchTerm"),
                    status = TempData.Peek("CurrentStatus"),
                    certification = TempData.Peek("CurrentCertification")
                });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while removing the question: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new
                {
                    pageIndex = pageIndex,
                    searchTerm = TempData.Peek("CurrentSearchTerm"),
                    status = TempData.Peek("CurrentStatus"),
                    certification = TempData.Peek("CurrentCertification")
                });
            }
        }


        [HttpGet]
        public async Task<IActionResult> Recover(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _questionRepository.GetByIdAsync(x => x.Id == id);
                await _questionRepository.RecoverAsync(entity);

                TempData["SuccessMessage"] = "Question recovered successfully.";
                return RedirectToAction("Index", new
                {
                    pageIndex = pageIndex,
                    searchTerm = TempData.Peek("CurrentSearchTerm"),
                    status = TempData.Peek("CurrentStatus"),
                    certification = TempData.Peek("CurrentCertification")
                });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while recovering the question: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new
                {
                    pageIndex = pageIndex,
                    searchTerm = TempData.Peek("CurrentSearchTerm"),
                    status = TempData.Peek("CurrentStatus"),
                    certification = TempData.Peek("CurrentCertification")
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Verify(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _questionRepository.GetByIdAsync(x => x.Id == id);
                entity.IsReported = false;

                await _questionRepository.UpdateAsync(entity);
                TempData["SuccessMessage"] = "Question recovered successfully.";
                return RedirectToAction("Index", new
                {
                    pageIndex = pageIndex,
                    searchTerm = TempData.Peek("CurrentSearchTerm"),
                    status = TempData.Peek("CurrentStatus"),
                    certification = TempData.Peek("CurrentCertification")
                });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while recovering the question: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new
                {
                    pageIndex = pageIndex,
                    searchTerm = TempData.Peek("CurrentSearchTerm"),
                    status = TempData.Peek("CurrentStatus"),
                    certification = TempData.Peek("CurrentCertification")
                });
            }
        }


        // Supportive Methods
        [HttpGet]
        public async Task<IActionResult> MoveQuestions()
        {
            try
            {
                ViewBag.Certifications = await _certificationRepository.GetAllAsync(x => !x.IsDeleted);
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading certifications: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> MoveQuestions(MoveDTO dto)
        {
            try
            {
                // Get all questions from the source certification, including drag-drop related data
                var questionsToMove = await _questionRepository.GetAllAsync(q => !q.IsDeleted && q.CertificationId == dto.FromCertificationId,
                    qi => qi.QuestionImages,
                    v => v.Variants,
                    d => d.DragDropQuestion,
                    c => c.Certification);

                if (!questionsToMove.Any())
                {
                    TempData["ErrorMessage"] = "No questions found for the selected certification.";
                    return RedirectToAction("Move");
                }

                foreach (var question in questionsToMove)
                {
                    var newQuestion = new Question
                    {
                        CertificationId = dto.ToCertificationId,
                        Content = question.Content,
                        Point = question.Point,
                        Server = question.Server,
                        Type = question.Type,
                        IsReported = question.IsReported,
                        Variants = question.Variants?.Select(v => new Variant
                        {
                            Text = v.Text,
                            IsCorrect = v.IsCorrect
                        }).ToList(),
                        QuestionImages = question.QuestionImages?.Select(img => new QuestionImage
                        {
                            ImageUrl = img.ImageUrl
                        }).ToList(),
                    };

                    await _questionRepository.AddAsync(newQuestion);

                    if (question.Type == EQuestionType.DragAndDrop && question.DragDropQuestion != null)
                    {
                        var dragVariantsAndDropZones = (await _dropZoneDragVariantRepository
                           .GetAllAsync(x => x.DragDropQuestionId == question.DragDropQuestion.Id,
                           dz => dz.DropZone,
                           dv => dv.DragVariant))
                           .ToList();

                        var newDragDropQuestion = new DragDropQuestion
                        {
                            QuestionId = newQuestion.Id, 
                            ImageUrl = question.DragDropQuestion.ImageUrl
                        };

                        await _dragDropQuestionRepository.AddAsync(newDragDropQuestion);

                        foreach (var dropZoneDragVariant in dragVariantsAndDropZones)
                        {
                            var newDropZone = new DropZone
                            {
                                X = dropZoneDragVariant.DropZone.X,
                                Y = dropZoneDragVariant.DropZone.Y,
                                Width = dropZoneDragVariant.DropZone.Width,
                                Height = dropZoneDragVariant.DropZone.Height
                            };
                            await _dropZoneRepository.AddAsync(newDropZone);

                            var newDragVariant = new DragVariant
                            {
                                ImageUrl = dropZoneDragVariant.DragVariant.ImageUrl
                            };
                            await _dragVariantRepository.AddAsync(newDragVariant);

                            var newDropZoneDragVariant = new DropZoneDragVariant
                            {
                                DragDropQuestionId = newDragDropQuestion.Id,
                                DropZoneId = newDropZone.Id,
                                DragVariantId = newDragVariant.Id
                            };

                            await _dropZoneDragVariantRepository.AddAsync(newDropZoneDragVariant);
                        }
                    }
                }

                TempData["SuccessMessage"] = $"{questionsToMove.Count} questions successfully copied from Certification {dto.FromCertificationId} to {dto.ToCertificationId}.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while copying questions: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Move");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadPdf()
        {
            try
            {
                ViewBag.Certifications = await _certificationRepository.GetAllAsync(x => !x.IsDeleted);
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading certifications: {ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExtractQuestionsWithAi(UploadPdfDTO uploadPdfDTO)
        {
            try
            {
                if (uploadPdfDTO.Pdf != null)
                {
                    var questions = await ExtractQuestionFromPdfWithCohere
                        .ProcessPdfInChunksAsync(uploadPdfDTO.Pdf, "command-r-plus-08-2024");

                    ExtractQuestionFromPdfWithCohere.RemoveQuestionPrefix(questions);
                    ExtractQuestionFromPdfWithCohere.RemoveInvalidPrefix(questions);

                    ViewBag.CertificationId = uploadPdfDTO.CertificationId;
                    return View("ShowQuestions", questions);
                }

                return RedirectToAction("UploadPdf");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while extracting questions with AI: {ex.InnerException?.Message}";
                return RedirectToAction("UploadPdf");
            }
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExtractQuestionsWithoutAi(UploadPdfDTO uploadPdfDTO)
        {
            try
            {
                if (uploadPdfDTO.Pdf != null)
                {
                    var questions = await ExtractQuestionFromPdf.ExtractQuestionsFromFileAsync(uploadPdfDTO.Pdf);
                    ExtractQuestionFromPdfWithCohere.RemoveQuestionPrefix(questions);
                    ExtractQuestionFromPdfWithCohere.RemoveInvalidPrefix(questions);
                    ViewBag.CertificationId = uploadPdfDTO.CertificationId;
                    return View("ShowQuestions", questions);
                }

                return RedirectToAction("UploadPdf");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while extracting questions without AI: {ex.InnerException?.Message}";
                return RedirectToAction("UploadPdf");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CalculateProcessingTime(IFormFile pdf)
        {
            if (pdf != null)
            {
                string extractedText = ExtractQuestionFromPdfWithCohere.ExtractTextFromPdf(pdf);

                if (string.IsNullOrEmpty(extractedText))
                {
                    return Json(new { estimatedTime = 0, message = "Failed to extract text from PDF." });
                }

                var timeInSeconds = ExtractQuestionFromPdfWithCohere.CalculateTime(extractedText);

                if (timeInSeconds == 0)
                {
                    return Json(new { estimatedTime = 0, message = "Failed to extract text from PDF." });
                }

                return Json(new { estimatedTime = timeInSeconds });
            }

            return Json(new { estimatedTime = 0, message = "No PDF file provided." });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult ShowQuestions(List<GetAllQuestionsFromPdfDTO> questions, int certificationId)
        {
            try
            {
                ViewBag.CertificationId = certificationId;

                return View(questions);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while showing the questions: {ex.InnerException?.Message}";
                return RedirectToAction("UploadPdf");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult ShowQuestionsPost(List<GetAllQuestionsFromPdfDTO> questions, int certificationId)
        {
            try
            {
                ViewBag.CertificationId = certificationId;
                return View("ShowQuestions", questions);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while showing the questions: {ex.InnerException?.Message}";
                return RedirectToAction("UploadPdf");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SaveQuestions(List<GetAllQuestionsFromPdfDTO> questions, int CertificationId)
        {
            try
            {
                if (questions == null || !questions.Any())
                {
                    TempData["ErrorMessage"] = "No questions were provided.";
                    return RedirectToAction("UploadPdf");
                }

                foreach (var questionDto in questions)
                {
                    if (questionDto.Variants is null || questionDto.Variants.Count == 0)
                    {
                        continue;
                    }

                    string cleanContent = Regex.Replace(questionDto.Content, @"^Question\s*#?\s*\d+:\s*", "", RegexOptions.IgnoreCase);
                    var doublePoint = Convert.ToDouble(questionDto.Point, CultureInfo.InvariantCulture);

                    var question = new Question
                    {
                        CertificationId = CertificationId,
                        Content = cleanContent,
                        Point = doublePoint,
                        Type = questionDto.Variants.Count(v => v.IsCorrect) > 1 ? EQuestionType.MultipleChoice : EQuestionType.SingleChoice,
                        Variants = new List<Variant>()
                    };

                    foreach (var variantDto in questionDto.Variants)
                    {
                        question.Variants.Add(new Variant
                        {
                            Text = variantDto.Text,
                            IsCorrect = variantDto.IsCorrect,
                            QuestionId = question.Id
                        });
                    }

                    await _appDbContext.Questions.AddAsync(question);
                }

                await _appDbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Questions saved successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while saving questions: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("UploadPdf");
            }
        }
    }
}
