using System.Text.RegularExpressions;
using AppTech.Business.DTOs.SettingDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.InternalServices.Interfaces;
using AppTech.Core.Entities;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class SettingService : ISettingService
    {
        protected readonly ISettingRepository _settingRepository;
        protected readonly ISettingHandler _settingHandler;
        protected readonly IHttpContextAccessor _http;
        protected readonly IMapper _mapper;

        public SettingService(
            ISettingRepository settingRepository,
            IMapper mapper,
            ISettingHandler settingHandler,
            IHttpContextAccessor http)
        {
            _settingRepository = settingRepository;
            _settingHandler = settingHandler;
            _mapper = mapper;
            _http = http;
        }

        public async Task<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>>>
GetAllAsync(SettingTypeDTO dto)
        {
            var supportedLanguages = new List<string> { "az", "en", "ru" };

            var entities = await _settingRepository.GetAllAsync(
                x => !x.IsDeleted,
                x => x.SettingTranslation);

            var settings = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>>();

            foreach (var language in supportedLanguages)
            {
                if (!settings.ContainsKey(language))
                {
                    settings[language] = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>();
                }

                foreach (var entity in entities)
                {
                    var translations = entity.SettingTranslation
                        .Where(st => st.Language.ToString().ToLower() == language && !st.IsDeleted)
                        .ToDictionary(
                            st => entity.Key,  
                            st => RemoveHtmlTags(st.Value.Trim('\"'))  
                        );

                    if (translations.Count == 0)
                        continue;

                    if (!settings[language].ContainsKey("translation"))
                    {
                        settings[language]["translation"] = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
                    }

                    if (!settings[language]["translation"].ContainsKey(entity.Page))
                    {
                        settings[language]["translation"][entity.Page] = new Dictionary<string, Dictionary<string, string>>();
                    }

                    if (!settings[language]["translation"][entity.Page].ContainsKey(entity.Type))
                    {
                        settings[language]["translation"][entity.Page][entity.Type] = new Dictionary<string, string>();
                    }

                    foreach (var translation in translations)
                    {
                        settings[language]["translation"][entity.Page][entity.Type][translation.Key] = translation.Value;
                    }
                }
            }

            return settings;
        }

        private string RemoveHtmlTags(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return Regex.Replace(input, "<.*?>", string.Empty);
        }


        public async Task<SettingDTO> GetByIdAsync(GetByIdSettingDTO dto)
        {
            var language = LanguageChanger.Change(new LanguageCatcher(_http).GetLanguage());

            var entity = _settingHandler.HandleEntityAsync(
                await _settingRepository.GetByIdAsync(
                    x => x.Id == dto.Id,
                    x => x.SettingTranslation));

            var translation = entity.SettingTranslation
                .FirstOrDefault(st => st.Language == language && !st.IsDeleted);

            return new SettingDTO
            {
                Id = entity.Id,
                Key = entity.Key,
                Type = entity.Type,
                Page = entity.Page,
                Value = translation is not null ? translation.Value.Trim('\"') : string.Empty,
                Translations = entity.SettingTranslation.Select(x => new SettingTranslationDTO
                {
                    Id = x.Id,
                    SettingId = x.SettingId,
                    Value = x.Value,
                    Language = x.Language.EnumToString()
                }).ToList(),
            };
        }


        public async Task<SettingDTO> AddAsync(CreateSettingDTO dto)
        {
            var newSetting = _mapper.Map<Setting>(dto);
            var entity = await _settingRepository.AddAsync(newSetting);

            return new SettingDTO
            {
                Id = entity.Id,
                Key = entity.Key
            };
        }

        public async Task<SettingDTO> DeleteAsync(DeleteSettingDTO dto)
        {
            var entity = await _settingRepository.DeleteAsync(
                _settingHandler.HandleEntityAsync(
                    await _settingRepository.GetByIdAsync(
                        x => x.Id == dto.Id)));

            return new SettingDTO
            {
                Id = entity.Id,
                Key = entity.Key
            };
        }

        public async Task<SettingDTO> UpdateAsync(UpdateSettingDTO dto)
        {
            var oldSetting = _settingHandler.HandleEntityAsync(
                await _settingRepository.GetByIdAsync(x => x.Id == dto.Id));

            oldSetting.Key = dto.Key ?? oldSetting.Key;
            oldSetting.Page = dto.Page ?? oldSetting.Page;

            var entity = await _settingRepository.UpdateAsync(oldSetting);

            return new SettingDTO
            {
                Id = entity.Id,
                Key = entity.Key,
            };
        }
    }
}
