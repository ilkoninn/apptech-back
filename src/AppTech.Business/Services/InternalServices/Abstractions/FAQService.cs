using AppTech.Business.DTOs.FAQDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class FAQService : IFAQService
    {
        protected readonly IFAQRepository _faqRepository;
        protected readonly IFAQHandler _faqHandler;
        protected readonly IHttpContextAccessor _http;
        protected readonly IMapper _mapper;

        public FAQService(IFAQRepository faqRepository, IMapper mapper,
            IConfiguration configuration, IFAQHandler fAQHandler, IHttpContextAccessor http)
        {
            _faqRepository = faqRepository;
            _mapper = mapper;
            _faqHandler = fAQHandler;
            _http = http;
        }

        public async Task<IQueryable<FAQDTO>> GetAllAsync(GetAllFAQDTO dto)
        {
            var language = LanguageChanger.Change(new LanguageCatcher(_http).GetLanguage());

            var entities = (await _faqRepository.GetAllAsync(
                x => x.IsDeleted == false))
                .Where(x => x.Language == language)
                .Select(e => new FAQDTO
                {
                    Id = e.Id,
                    Question = e.Question,
                    Answer = e.Answer,
                    Language = language.EnumToString()
                });

            return entities.AsQueryable();
        }

        public async Task<FAQDTO> GetByIdAsync(GetByIdFAQDTO dto)
        {
            var entity = _faqHandler.HandleEntityAsync(
                await _faqRepository.GetByIdAsync(x => x.Id == dto.Id));

            return new FAQDTO
            {
                Id = entity.Id,
                Question = entity.Question,
                Answer = entity.Answer,
                Language = entity.Language.EnumToString()
            };
        }

        public async Task<FAQDTO> AddAsync(CreateFAQDTO dto)
        {
            var entity = await _faqRepository.AddAsync(_mapper.Map<FAQ>(dto));

            return new FAQDTO
            {
                Id = entity.Id,
                Question = entity.Question,
                Answer = entity.Answer,
                Language = entity.Language.EnumToString()
            };
        }

        public async Task<FAQDTO> DeleteAsync(DeleteFAQDTO dto)
        {
            var entity = await _faqRepository.DeleteAsync(
                _faqHandler.HandleEntityAsync(
                await _faqRepository.GetByIdAsync(x => x.Id == dto.Id)));

            return new FAQDTO
            {
                Id = entity.Id,
                Question = entity.Question,
                Answer = entity.Answer,
                Language = entity.Language.EnumToString()
            };
        }

        public async Task<FAQDTO> UpdateAsync(UpdateFAQDTO dto)
        {
            var entity = await _faqRepository.UpdateAsync(_mapper.Map(dto,
                _faqHandler.HandleEntityAsync(
                await _faqRepository.GetByIdAsync(x => x.Id == dto.Id))));

            return new FAQDTO
            {
                Id = entity.Id,
                Question = entity.Question,
                Answer = entity.Answer,
                Language = entity.Language.EnumToString()
            };
        }
    }
}
