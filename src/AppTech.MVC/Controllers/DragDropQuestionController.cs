using AppTech.Business.DTOs.QuestionDTOs;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Enums;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.MVC.Controllers
{
    public class DragDropQuestionController : BaseController
    {
        private readonly ICertificationRepository _certificationRepository;
        private readonly IDragDropQuestionRepository _dragDropQuestionRepository;
        private readonly IDropZoneDragVariantRepository _dropZoneDragVariantsRepository;
        private readonly IDragVariantRepository _dragVariantRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IDropZoneRepository _dropZoneRepository;
        private readonly IFileManagerService _fileManagerService;
        private readonly AppDbContext _dbContext;

        public DragDropQuestionController(ICertificationRepository certificationRepository,
            IFileManagerService fileManagerService,
            IQuestionRepository questionRepository,
            AppDbContext appDbContext,
            IDragDropQuestionRepository dragDropQuestionRepository,
            IDropZoneRepository dropZoneRepository,
            IDragVariantRepository dragVariantRepository,
            IDropZoneDragVariantRepository dropZoneDragVariantsRepository)
        {
            _certificationRepository = certificationRepository;
            _dragDropQuestionRepository = dragDropQuestionRepository;
            _fileManagerService = fileManagerService;
            _questionRepository = questionRepository;
            _dbContext = appDbContext;
            _dropZoneRepository = dropZoneRepository;
            _dragVariantRepository = dragVariantRepository;
            _dropZoneDragVariantsRepository = dropZoneDragVariantsRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var certifications = await _certificationRepository.GetAllAsync(x => x.IsDeleted == false);
            ViewBag.Certifications = certifications.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Title} v{c.LastVersion}",
            }).ToList();

            return View(new DragDropQuestionDTO());
        }

        [HttpPost]
        public async Task<IActionResult> Create(DragDropQuestionDTO model)
        {
            if (!ModelState.IsValid || model.DropZones == null || !model.DropZones.Any())
            {
                TempData["ErrorMessage"] = "At least one drop zone must be provided.";
                return View(model);
            }

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var question = new Question
                    {
                        CertificationId = model.CertificationId,
                        Content = model.QuestionText,
                        Point = model.Point,
                        Type = EQuestionType.DragAndDrop,
                    };

                    string imageUrl = await _fileManagerService.UploadFileAsync(model.Image);
                    await _questionRepository.AddAsync(question);

                    var dragDropQuestion = new DragDropQuestion
                    {
                        QuestionId = question.Id,
                        ImageUrl = imageUrl
                    };

                    await _dragDropQuestionRepository.AddAsync(dragDropQuestion);

                    foreach (var dropZoneDto in model.DropZones)
                    {
                        var dropZone = new DropZone
                        {
                            X = dropZoneDto.X,
                            Y = dropZoneDto.Y,
                            Width = dropZoneDto.Width,
                            Height = dropZoneDto.Height,
                        };

                        var newDropZone = await _dropZoneRepository.AddAsync(dropZone);

                        string variantImageUrl = await _fileManagerService.UploadFileAsync(dropZoneDto.Image);

                        var dragVariant = new DragVariant
                        {
                            ImageUrl = variantImageUrl
                        };

                        var newDragVariant = await _dragVariantRepository.AddAsync(dragVariant);

                        var dropZoneDragVariant = new DropZoneDragVariant
                        {
                            DragDropQuestionId = dragDropQuestion.Id,
                            DragVariantId = newDragVariant.Id,
                            DropZoneId = newDropZone.Id
                        };

                        await _dropZoneDragVariantsRepository.AddAsync(dropZoneDragVariant);
                    }

                    await transaction.CommitAsync();
                    TempData["SuccessMessage"] = "Saved successfully!";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = ex.InnerException?.Message ?? ex.Message;
                    return View(model);
                }
            }

            return Redirect("/Question/Index");
        }
    }
}
