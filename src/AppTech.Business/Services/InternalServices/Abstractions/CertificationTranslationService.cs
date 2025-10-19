using AppTech.Business.DTOs.CertificationDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class CertificationTranslationService : ICertificationTranslationService
    {
        protected readonly ICertificationTranslationRepository _certificationTranslationRepository;
        protected readonly ICertificationTranslationHandler _certificationTranslationHandler;
        protected readonly ICertificationHandler _certificationHandler;
        protected readonly ICertificationRepository _certificationRepository;
        protected readonly IMapper _mapper;

        public CertificationTranslationService(ICertificationTranslationRepository CertificationTranslationRepository, IMapper mapper, ICertificationTranslationHandler certificationTranslationHandler, ICertificationHandler certificationHandler, ICertificationRepository certificationRepository)
        {
            _certificationTranslationRepository = CertificationTranslationRepository;
            _certificationTranslationHandler = certificationTranslationHandler;
            _certificationHandler = certificationHandler;
            _certificationRepository = certificationRepository;
            _mapper = mapper;
        }

        public async Task<IQueryable<CertificationTranslationDTO>> GetAllAsync()
        {
            var entities = (await _certificationTranslationRepository.GetAllAsync(
                x => x.IsDeleted == false))
                .Select(e => new CertificationTranslationDTO
                {
                    Id = e.Id,
                    CertificationId = e.CertificationId,
                    Description = e.Description,
                    Language = e.Language.EnumToString(),
                });

            return entities.AsQueryable();
        }

        public async Task<CertificationTranslationDTO> GetByIdAsync(GetByIdCertificationTranslationDTO dto)
        {
            var entity = _certificationTranslationHandler.HandleEntityAsync(
                await _certificationTranslationRepository.GetByIdAsync(x => x.Id == dto.Id));

            return new CertificationTranslationDTO
            {
                Id = entity.Id,
                CertificationId = entity.CertificationId,
                Description = entity.Description,
                Language = entity.Language.EnumToString(),
            };
        }

        public async Task<CertificationTranslationDTO> AddAsync(CreateCertificationTranslationDTO dto)
        {
            var entity = await _certificationTranslationRepository.AddAsync(_mapper.Map<CertificationTranslation>(dto));

            return new CertificationTranslationDTO
            {
                Id = entity.Id,
                CertificationId = entity.CertificationId,
                Description = entity.Description,
                Language = entity.Language.EnumToString(),
            };
        }

        public async Task<CertificationTranslationDTO> DeleteAsync(DeleteCertificationTranslationDTO dto)
        {
            var entity = await _certificationTranslationRepository.DeleteAsync(
                _certificationTranslationHandler.HandleEntityAsync(
                await _certificationTranslationRepository.GetByIdAsync(x => x.Id == dto.Id)));

            return new CertificationTranslationDTO
            {
                Id = entity.Id,
                CertificationId = entity.CertificationId,
                Description = entity.Description,
                Language = entity.Language.EnumToString(),
            };
        }

        public async Task<CertificationTranslationDTO> UpdateAsync(UpdateCertificationTranslationDTO dto)
        {
            var entity = await _certificationTranslationRepository.UpdateAsync(_mapper.Map(dto,
                _certificationTranslationHandler.HandleEntityAsync(
                await _certificationTranslationRepository.GetByIdAsync(x => x.Id == dto.Id))));

            return new CertificationTranslationDTO
            {
                Id = entity.Id,
                CertificationId = entity.CertificationId,
                Description = entity.Description,
                Language = entity.Language.EnumToString(),
            };
        }

        public async Task CheckNotFoundCertificationAsync(int certificationId)
        {
            _certificationHandler.HandleEntityAsync(
            await _certificationRepository.GetByIdAsync(x => x.Id == certificationId && !x.IsDeleted));
        }
    }
}
