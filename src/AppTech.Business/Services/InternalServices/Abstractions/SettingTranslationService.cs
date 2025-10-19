using AppTech.Business.DTOs.SettingDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.InternalServices.Interfaces;
using AppTech.Core.Entities;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class SettingTranslationService : ISettingTranslationService
    {
        protected readonly ISettingTranslationRepository _SettingTranslationRepository;
        protected readonly IMapper _mapper;

        public SettingTranslationService(
            ISettingTranslationRepository SettingTranslationRepository,
            IMapper mapper)
        {
            _SettingTranslationRepository = SettingTranslationRepository;
            _mapper = mapper;
        }

        public async Task<IQueryable<SettingTranslationDTO>> GetAllAsync()
        {
            var entities = await _SettingTranslationRepository.GetAllAsync(x => !x.IsDeleted);

            var dtos = entities.Select(e => new SettingTranslationDTO
            {
                Id = e.Id,
                Value = e.Value,
                Language = e.Language.EnumToString(),
                SettingId = e.SettingId,
            });

            return dtos.AsQueryable();
        }

        public async Task<SettingTranslationDTO> GetByIdAsync(GetByIdSettingTranslationDTO dto)
        {
            var entity = await _SettingTranslationRepository.GetByIdAsync(x => x.Id == dto.Id);

            return new SettingTranslationDTO
            {
                Id = entity.Id,
                Value = entity.Value,
                Language = entity.Language.EnumToString(),
                SettingId = entity.SettingId,
            };
        }

        public async Task<SettingTranslationDTO> AddAsync(CreateSettingTranslationDTO dto)
        {
            var entity = await _SettingTranslationRepository.AddAsync(_mapper.Map<SettingTranslation>(dto));

            return new SettingTranslationDTO
            {
                Id = entity.Id,
                Value = entity.Value,
                Language = entity.Language.EnumToString(),
                SettingId = entity.SettingId,
            };
        }

        public async Task<SettingTranslationDTO> UpdateAsync(UpdateSettingTranslationDTO dto)
        {
            var entity = await _SettingTranslationRepository.UpdateAsync(_mapper.Map<SettingTranslation>(dto));

            return new SettingTranslationDTO
            {
                Id = entity.Id,
                Value = entity.Value,
                Language = entity.Language.EnumToString(),
                SettingId = entity.SettingId,
            };
        }

        public async Task<SettingTranslationDTO> DeleteAsync(DeleteSettingTranslationDTO dto)
        {
            var entity = await _SettingTranslationRepository.DeleteAsync(
                await _SettingTranslationRepository.GetByIdAsync(x => x.Id == dto.Id));

            return new SettingTranslationDTO
            {
                Id = entity.Id,
                Value = entity.Value,
                Language = entity.Language.EnumToString(),
                SettingId = entity.SettingId,
            };
        }
    }
}
