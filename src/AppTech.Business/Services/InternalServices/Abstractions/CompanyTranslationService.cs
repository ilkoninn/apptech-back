using AppTech.Business.DTOs.CompanyDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class CompanyTranslationService : ICompanyTranslationService
    {
        protected readonly ICompanyTranslationRepository _companyTranslationRepository;
        protected readonly ICompanyTranslationHandler _companyTranslationHandler;
        protected readonly ICompanyHandler _companyHandler;
        protected readonly ICompanyRepository _companyRepository;
        protected readonly IMapper _mapper;
        public CompanyTranslationService(ICompanyTranslationRepository CompanyTranslationRepository, IMapper mapper, ICompanyTranslationHandler companyTranslationHandler, ICompanyHandler companyHandler = null, ICompanyRepository companyRepository = null)
        {
            _companyTranslationRepository = CompanyTranslationRepository;
            _companyTranslationHandler = companyTranslationHandler;
            _mapper = mapper;
            _companyHandler = companyHandler;
            _companyRepository = companyRepository;
        }

        public async Task<IQueryable<CompanyTranslationDTO>> GetAllAsync()
        {
            var entities = (await _companyTranslationRepository.GetAllAsync(
                x => x.IsDeleted == false))
                .Select(e => new CompanyTranslationDTO
                {
                    Id = e.Id,
                    CompanyId = e.CompanyId,
                    SecTitle = e.SecTitle,
                    SecDescription = e.SecDescription,
                    Language = e.Language.EnumToString(),
                });

            return entities.AsQueryable();
        }

        public async Task<CompanyTranslationDTO> GetByIdAsync(GetByIdCompanyTranslationDTO dto)
        {
            var entity = _companyTranslationHandler.HandleEntityAsync(
                await _companyTranslationRepository.GetByIdAsync(x => x.Id == dto.Id));

            return new CompanyTranslationDTO
            {
                Id = entity.Id,
                CompanyId = entity.CompanyId,
                SecTitle = entity.SecTitle,
                SecDescription = entity.SecDescription,
                Language = entity.Language.EnumToString(),
            };
        }

        public async Task<CompanyTranslationDTO> AddAsync(CreateCompanyTranslationDTO dto)
        {
            var entity = await _companyTranslationRepository.AddAsync(_mapper.Map<CompanyTranslation>(dto));

            return new CompanyTranslationDTO
            {
                Id = entity.Id,
                CompanyId = entity.CompanyId,
                SecTitle = entity.SecTitle,
                SecDescription = entity.SecDescription,
                Language = entity.Language.EnumToString(),
            };
        }

        public async Task<CompanyTranslationDTO> DeleteAsync(DeleteCompanyTranslationDTO dto)
        {
            var entity = await _companyTranslationRepository.DeleteAsync(
                _companyTranslationHandler.HandleEntityAsync(
                await _companyTranslationRepository.GetByIdAsync(x => x.Id == dto.Id)));

            return new CompanyTranslationDTO
            {
                Id = entity.Id,
                CompanyId = entity.CompanyId,
                SecTitle = entity.SecTitle,
                SecDescription = entity.SecDescription,
                Language = entity.Language.EnumToString(),
            };
        }

        public async Task<CompanyTranslationDTO> UpdateAsync(UpdateCompanyTranslationDTO dto)
        {
            var entity = await _companyTranslationRepository.UpdateAsync(_mapper.Map(dto,
                _companyTranslationHandler.HandleEntityAsync(
                await _companyTranslationRepository.GetByIdAsync(x => x.Id == dto.Id))));

            return new CompanyTranslationDTO
            {
                Id = entity.Id,
                CompanyId = entity.CompanyId,
                SecTitle = entity.SecTitle,
                SecDescription = entity.SecDescription,
                Language = entity.Language.EnumToString(),
            };
        }

        public async Task CheckNotFoundCompanyAsync(int companyId)
        {
            _companyHandler.HandleEntityAsync(
            await _companyRepository.GetByIdAsync(x => x.Id == companyId && !x.IsDeleted));
        }
    }
}
