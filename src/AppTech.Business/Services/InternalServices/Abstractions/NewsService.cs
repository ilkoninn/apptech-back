using AppTech.Business.DTOs.NewsDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class NewsService : INewsService
    {
        protected readonly INewsRepository _newsRepository;
        protected readonly IHttpContextAccessor _http;
        protected readonly INewsHandler _newsHandler;
        protected readonly IMapper _mapper;

        public NewsService(INewsRepository NewsRepository, IMapper mapper, INewsHandler newsHandler, IHttpContextAccessor http)
        {
            _newsRepository = NewsRepository;
            _newsHandler = newsHandler;
            _mapper = mapper;
            _http = http;
        }

        public async Task<IQueryable<NewsDTO>> GetAllAsync(GetAllNewsDTO dto)
        {
            var language = LanguageChanger.Change(new LanguageCatcher(_http).GetLanguage());

            var entities = (await _newsRepository.GetAllAsync(
                x => x.IsDeleted == false,
                x => x.NewsTranslations))
                .Select(e => new NewsDTO
                {
                    Id = e.Id,
                    Title = e.Title,
                    ImageUrl = e.ImageUrl,
                    Url = e.Url,
                    Description = e.NewsTranslations
                    .Where(ct => ct.Language == language && !ct.IsDeleted)
                    .Select(ct => ct.Description)
                    .FirstOrDefault(),
                });

            return entities.AsQueryable();
        }

        public async Task<NewsDTO> GetByIdAsync(GetByIdNewsDTO dto)
        {
            var entity = _newsHandler.HandleEntityAsync(
                await _newsRepository.GetByIdAsync(x => x.Id == dto.Id, nt => nt.NewsTranslations));

            return new NewsDTO
            {
                Id = entity.Id,
                Title = entity.Title,
                ImageUrl = entity.ImageUrl,
                Url = entity.Url,
                Translations = entity.NewsTranslations.Select(ct => new NewsTranslationDTO
                {
                    Id = ct.Id,
                    NewsId = ct.NewsId,
                    Description = ct.Description,
                    Language = EnumExtensions.EnumToString(ct.Language),
                }).ToList(),
            };
        }

        public async Task<NewsDTO> AddAsync(CreateNewsDTO dto)
        {
            var entity = await _newsRepository.AddAsync(_mapper.Map<News>(dto));

            return new NewsDTO
            {
                Id = entity.Id,
                Title = entity.Title,
                ImageUrl = entity.ImageUrl,
                Url = entity.Url,
            };
        }

        public async Task<NewsDTO> DeleteAsync(DeleteNewsDTO dto)
        {
            var entity = await _newsRepository.DeleteAsync(
                _newsHandler.HandleEntityAsync(
                await _newsRepository.GetByIdAsync(x => x.Id == dto.Id)));

            return new NewsDTO
            {
                Id = entity.Id,
                Title = entity.Title,
                ImageUrl = entity.ImageUrl,
                Url = entity.Url,
            };
        }

        public async Task<NewsDTO> UpdateAsync(UpdateNewsDTO dto)
        {
            var entity = await _newsRepository.UpdateAsync(_mapper.Map(dto,
                _newsHandler.HandleEntityAsync(
                await _newsRepository.GetByIdAsync(x => x.Id == dto.Id))));

            return new NewsDTO
            {
                Id = entity.Id,
                Title = entity.Title,
                ImageUrl = entity.ImageUrl,
                Url = entity.Url,
            };
        }
    }
}
