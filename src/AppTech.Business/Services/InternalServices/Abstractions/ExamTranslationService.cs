using AppTech.Business.DTOs.ExamDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class ExamTranslationService : IExamTranslationService
    {
        protected readonly IExamTranslationRepository _examTranslationRepository;
        protected readonly IExamTranslationHandler _examTranslationHandler;
        protected readonly IExamHandler _examHandler;
        protected readonly IExamRepository _examRepository;
        protected readonly IMapper _mapper;
        public ExamTranslationService(IExamTranslationRepository ExamTranslationRepository, IMapper mapper, IExamTranslationHandler ExamTranslationHandler, IExamHandler examHandler, IExamRepository examRepository)
        {
            _examTranslationRepository = ExamTranslationRepository;
            _examTranslationHandler = ExamTranslationHandler;
            _examHandler = examHandler;
            _examRepository = examRepository;
            _mapper = mapper;
        }

        public async Task<IQueryable<ExamTranslationDTO>> GetAllAsync()
        {
            var entities = (await _examTranslationRepository.GetAllAsync(
                x => x.IsDeleted == false))
                .Select(e => new ExamTranslationDTO
                {
                    Id = e.Id,
                    ExamId = e.ExamId,
                    Description = e.Description,
                    Language = e.Language.EnumToString(),
                });

            return entities.AsQueryable();
        }

        public async Task<ExamTranslation> GetByIdAsync(GetByIdExamTranslationDTO dto)
        {
            return _examTranslationHandler.HandleEntityAsync(
                await _examTranslationRepository.GetByIdAsync(x => x.Id == dto.Id));
        }

        public async Task<ExamTranslationDTO> AddAsync(CreateExamTranslationDTO dto)
        {
            var entity = await _examTranslationRepository.AddAsync(_mapper.Map<ExamTranslation>(dto));

            return new ExamTranslationDTO
            {
                Id = entity.Id,
                ExamId = entity.ExamId,
                Description = entity.Description,
                Language = entity.Language.EnumToString(),
            };
        }

        public async Task<ExamTranslationDTO> DeleteAsync(DeleteExamTranslationDTO dto)
        {
            var entity = await _examTranslationRepository.DeleteAsync(
                _examTranslationHandler.HandleEntityAsync(
                await _examTranslationRepository.GetByIdAsync(x => x.Id == dto.Id)));

            return new ExamTranslationDTO
            {
                Id = entity.Id,
                ExamId = entity.ExamId,
                Description = entity.Description,
                Language = entity.Language.EnumToString(),
            };
        }

        public async Task<ExamTranslationDTO> UpdateAsync(UpdateExamTranslationDTO dto)
        {
            var entity = await _examTranslationRepository.UpdateAsync(_mapper.Map(dto,
                _examTranslationHandler.HandleEntityAsync(
                await _examTranslationRepository.GetByIdAsync(x => x.Id == dto.Id))));

            return new ExamTranslationDTO
            {
                Id = entity.Id,
                ExamId = entity.ExamId,
                Description = entity.Description,
                Language = entity.Language.EnumToString(),
            };
        }

        public async Task CheckNotFoundExamAsync(int examId)
        {
            _examHandler.HandleEntityAsync(
            await _examRepository.GetByIdAsync(x => x.Id == examId));
        }
    }
}
