using AppTech.Business.DTOs.QuestionDTOs;
using AppTech.Business.DTOs.VariantDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class QuestionService : IQuestionService
    {
        protected readonly IQuestionImageRepository _questionImageRepository;
        protected readonly IQuestionRepository _questionRepository;
        protected readonly IFileManagerService _fileManagerService;
        protected readonly IVariantRepository _variantRepository;
        protected readonly IQuestionHandler _questionHandler;
        protected readonly IAccountService _accountService;
        protected readonly IMapper _mapper;

        public QuestionService(IQuestionRepository QuestionRepository, IMapper mapper, IQuestionHandler questionHandler, IFileManagerService fileManagerService, IQuestionImageRepository questionImageRepository, IVariantRepository variantRepository, IAccountService accountService)
        {
            _questionImageRepository = questionImageRepository;
            _questionRepository = QuestionRepository;
            _fileManagerService = fileManagerService;
            _questionHandler = questionHandler;
            _mapper = mapper;
            _variantRepository = variantRepository;
            _accountService = accountService;
        }

        public async Task<IQueryable<QuestionDTO>> GetAllQuestionWithVariantsAsync(GetAllBySlugDTO dto)
        {
            var entities = await _questionRepository
                .GetAllAsync(x => !x.IsDeleted && x.Certification.Slug == dto.slug,
                v => v.Variants,
                c => c.Certification,
                qi => qi.QuestionImages);

            var hasRedHatQuestions = entities.Any(q => q.Server != null);

            List<QuestionDTO> orderedQuestions;

            if (hasRedHatQuestions)
            {
                var introQuestions = entities.Where(q => q.Server == 0).ToList();

                var remainingQuestions = entities.Where(q => q.Server > 0).ToList();

                var introQuestionDTOs = introQuestions.Select(q => new QuestionDTO
                {
                    Id = q.Id,
                    Content = q.Content,
                    Type = EnumExtensions.EnumToString(q.Type),
                    ImageUrls = q.QuestionImages.Select(x => x.ImageUrl).ToList(),
                    Variants = q.Variants.Select(v => new VariantDTO
                    {
                        Id = v.Id,
                        Text = v.Text,
                        IsCorrect = v.IsCorrect
                    }).ToList(),
                }).ToList();

                var remainingQuestionDTOs = remainingQuestions.Select(q => new QuestionDTO
                {
                    Id = q.Id,
                    Content = q.Content,
                    Type = EnumExtensions.EnumToString(q.Type),
                    ImageUrls = q.QuestionImages.Select(x => x.ImageUrl).ToList(),
                    Variants = q.Variants.Select(v => new VariantDTO
                    {
                        Id = v.Id,
                        Text = v.Text,
                        IsCorrect = v.IsCorrect
                    }).ToList(),
                }).ToList();

                orderedQuestions = introQuestionDTOs.Concat(remainingQuestionDTOs).ToList();
            }
            else
            {
                orderedQuestions = entities.Select(q => new QuestionDTO
                {
                    Id = q.Id,
                    Content = q.Content,
                    Type = EnumExtensions.EnumToString(q.Type),
                    ImageUrls = q.QuestionImages.Select(x => x.ImageUrl).ToList(),
                    Variants = q.Variants.Select(v => new VariantDTO
                    {
                        Id = v.Id,
                        Text = v.Text,
                        IsCorrect = v.IsCorrect
                    }).ToList(),
                }).ToList();
            }

            return orderedQuestions.AsQueryable();
        }


        public async Task ReportTheSelectedQuestionAsync(ReportQuestionDTO dto)
        {
            var oldQuestion = await _questionRepository.GetByIdAsync(x => !x.IsDeleted && x.Id == dto.questionId);
            var user = await _accountService.CheckNotFoundByIdAsync(dto.userId);
            var isConfirm = user.EmailConfirmed == true ? "Confirmed" : "Not confirmed";

            oldQuestion.IsReported = true;
            oldQuestion.ReportFrom = $"{user.FullName ?? user.UserName} | {user.Email} ({isConfirm})";
            await _questionRepository.UpdateAsync(oldQuestion);
        }

        // CRUD Section
        public async Task<IQueryable<QuestionDTO>> GetAllAsync()
        {
            var entities = await _questionRepository.GetAllAsync(x => x.IsDeleted == false);

            return entities.Select(q => new QuestionDTO
            {
                Id = q.Id,
                Content = q.Content,
            }).AsQueryable();
        }

        public async Task<QuestionDTO> GetByIdAsync(GetByIdQuestionDTO dto)
        {
            var entity = _questionHandler.HandleEntityAsync(
                await _questionRepository.GetByIdAsync(x => x.Id == dto.Id));

            return new QuestionDTO
            {
                Id = entity.Id,
                Content = entity.Content,
            };
        }

        public async Task<QuestionDTO> AddAsync(CreateQuestionDTO dto)
        {
            var map = _mapper.Map<Question>(dto);

            map.QuestionImages = new List<QuestionImage>();

            if (dto.Images != null)
            {
                foreach (var image in dto.Images)
                {
                    var imageUrl = await _fileManagerService.UploadFileAsync(image);

                    map.QuestionImages.Add(new QuestionImage
                    {
                        QuestionId = map.Id,
                        ImageUrl = imageUrl,
                    });
                }
            }

            var entity = await _questionRepository.AddAsync(map);

            return new QuestionDTO
            {
                Id = entity.Id,
                Content = entity.Content,
            };
        }

        public async Task<QuestionDTO> DeleteAsync(DeleteQuestionDTO dto)
        {
            var entity = await _questionRepository.DeleteAsync(
                _questionHandler.HandleEntityAsync(
                await _questionRepository.GetByIdAsync(x => x.Id == dto.Id)));

            return new QuestionDTO
            {
                Id = entity.Id,
                Content = entity.Content,
            };
        }

        public async Task<QuestionDTO> UpdateAsync(UpdateQuestionDTO dto)
        {
            var oldQuestion = await _questionRepository.GetByIdAsync(x => x.Id == dto.Id);

            if (oldQuestion == null)
            {
                throw new Exception("Question not found.");
            }

            oldQuestion = _mapper.Map<Question>(dto);

            if (dto.Images != null && dto.Images.Any())
            {
                foreach (var image in dto.Images)
                {
                    var imageUrl = await _fileManagerService.UploadFileAsync(image);

                    oldQuestion.QuestionImages.Add(new QuestionImage
                    {
                        QuestionId = oldQuestion.Id,
                        ImageUrl = imageUrl,
                    });
                }
            }

            oldQuestion.Variants = new List<Variant>();

            await _questionRepository.UpdateAsync(oldQuestion);

            return new QuestionDTO
            {
                Id = oldQuestion.Id,
                Content = oldQuestion.Content,
            };
        }
    }
}
