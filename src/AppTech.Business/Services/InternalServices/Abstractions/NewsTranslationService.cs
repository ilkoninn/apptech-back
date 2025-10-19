using AppTech.Business.DTOs.NewsDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class NewsTranslationService : INewsTranslationService
    {
        protected readonly INewsTranslationRepository _newsTranslationRepository;
        protected readonly INewsTranslationHandler _newsTranslationHandler;
        protected readonly INewsHandler _newsHandler;
        protected readonly INewsRepository _newsRepository;
        protected readonly IMapper _mapper;
        public NewsTranslationService(INewsTranslationRepository NewsTranslationRepository, IMapper mapper, INewsTranslationHandler NewsTranslationHandler, INewsHandler newsHandler, INewsRepository newsRepository)
        {
            _newsTranslationRepository = NewsTranslationRepository;
            _newsTranslationHandler = NewsTranslationHandler;
            _newsHandler = newsHandler;
            _newsRepository = newsRepository;
            _mapper = mapper;
        }

        public async Task<IQueryable<NewsTranslationDTO>> GetAllAsync()
        {
            var entities = (await _newsTranslationRepository.GetAllAsync(
                x => x.IsDeleted == false))
                .Select(e => new NewsTranslationDTO
                {
                    Id = e.Id,
                    NewsId = e.NewsId,
                    Description = e.Description,
                    Language = e.Language.EnumToString(),
                });

            return entities.AsQueryable();
        }

        public async Task<NewsTranslationDTO> GetByIdAsync(GetByIdNewsTranslationDTO dto)
        {
            var entity = _newsTranslationHandler.HandleEntityAsync(
                await _newsTranslationRepository.GetByIdAsync(x => x.Id == dto.Id));

            return new NewsTranslationDTO
            {
                Id = entity.Id,
                NewsId = entity.NewsId,
                Description = entity.Description,
                Language = entity.Language.EnumToString(),
            };
        }

        public async Task<NewsTranslationDTO> AddAsync(CreateNewsTranslationDTO dto)
        {
            var entity = await _newsTranslationRepository.AddAsync(_mapper.Map<NewsTranslation>(dto));

            return new NewsTranslationDTO
            {
                Id = entity.Id,
                NewsId = entity.NewsId,
                Description = entity.Description,
                Language = entity.Language.EnumToString(),
            };
        }

        public async Task<NewsTranslationDTO> DeleteAsync(DeleteNewsTranslationDTO dto)
        {
            var entity = await _newsTranslationRepository.DeleteAsync(
                _newsTranslationHandler.HandleEntityAsync(
                await _newsTranslationRepository.GetByIdAsync(x => x.Id == dto.Id)));

            return new NewsTranslationDTO
            {
                Id = entity.Id,
                NewsId = entity.NewsId,
                Description = entity.Description,
                Language = entity.Language.EnumToString(),
            };
        }

        public async Task<NewsTranslationDTO> UpdateAsync(UpdateNewsTranslationDTO dto)
        {
            var entity = await _newsTranslationRepository.UpdateAsync(_mapper.Map(dto,
                _newsTranslationHandler.HandleEntityAsync(
                await _newsTranslationRepository.GetByIdAsync(x => x.Id == dto.Id))));

            return new NewsTranslationDTO
            {
                Id = entity.Id,
                NewsId = entity.NewsId,
                Description = entity.Description,
                Language = entity.Language.EnumToString(),
            };
        }

        public async Task CheckNotFoundNewsAsync(int newsId)
        {
            _newsHandler.HandleEntityAsync(
            await _newsRepository.GetByIdAsync(x => x.Id == newsId));
        }
    }
}
