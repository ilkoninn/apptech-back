using AppTech.Business.DTOs.PartnerDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.InternalServices.Interfaces;
using AppTech.Core.Entities;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class PartnerService : IPartnerService
    {
        protected readonly IPartnerRepository _partnerRepository;
        protected readonly IPartnerHandler _partnerHandler;
        protected readonly IHttpContextAccessor _http;
        protected readonly IMapper _mapper;

        public PartnerService(IPartnerRepository partnerRepository, IMapper mapper,
            IPartnerHandler partnerHandler, IHttpContextAccessor http)
        {
            _partnerRepository = partnerRepository;
            _partnerHandler = partnerHandler;
            _mapper = mapper;
            _http = http;
        }

        public async Task<IQueryable<PartnerDTO>> GetAllAsync()
        {
            var language = LanguageChanger.Change(new LanguageCatcher(_http).GetLanguage());

            var entities = (await _partnerRepository.GetAllAsync(
                x => x.IsDeleted == false && x.IsAccepted))
                .Select(e => new PartnerDTO
                {
                    Id = e.Id,
                    Title = e.Company,
                    ImageUrl = e.ImageUrl,
                    Url = e.Url,
                });

            return entities.AsQueryable();
        }

        public async Task<PartnerDTO> GetByIdAsync(GetByIdPartnerDTO dto)
        {
            var entity = _partnerHandler.HandleEntityAsync(
                await _partnerRepository.GetByIdAsync(x => x.Id == dto.Id));

            return new PartnerDTO
            {
                Id = entity.Id,
                Title = entity.Company,
                ImageUrl = entity.ImageUrl,
                Url = entity.Url,
            };
        }

        public async Task<PartnerDTO> AddPartnerOnlineAsync(CreatePartnerOnlineDTO dto)
        {
            var entity = await _partnerRepository.AddAsync(_mapper.Map<Partner>(dto));

            return new PartnerDTO
            {
                Id = entity.Id,
                Title = entity.Company,
                ImageUrl = entity.ImageUrl,
                Url = entity.Url,
            };
        }

        public async Task<PartnerDTO> AddAsync(CreatePartnerDTO dto)
        {
            var entity = await _partnerRepository.AddAsync(_mapper.Map<Partner>(dto));

            return new PartnerDTO
            {
                Id = entity.Id,
                Title = entity.Company,
                ImageUrl = entity.ImageUrl,
                Url = entity.Url,
            };
        }

        public async Task<PartnerDTO> DeleteAsync(DeletePartnerDTO dto)
        {
            var entity = await _partnerRepository.DeleteAsync(
                _partnerHandler.HandleEntityAsync(
                await _partnerRepository.GetByIdAsync(x => x.Id == dto.Id)));

            return new PartnerDTO
            {
                Id = entity.Id,
                Title = entity.Company,
                ImageUrl = entity.ImageUrl,
                Url = entity.Url,
            };
        }

        public async Task<PartnerDTO> UpdateAsync(UpdatePartnerDTO dto)
        {
            var entity = await _partnerRepository.UpdateAsync(_mapper.Map(dto,
                _partnerHandler.HandleEntityAsync(
                await _partnerRepository.GetByIdAsync(x => x.Id == dto.Id))));

            return new PartnerDTO
            {
                Id = entity.Id,
                Title = entity.Company,
                ImageUrl = entity.ImageUrl,
                Url = entity.Url,
            };
        }
    }
}
