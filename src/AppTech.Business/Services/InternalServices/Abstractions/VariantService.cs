using AppTech.Business.DTOs.VariantDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class VariantService : IVariantService
    {
        protected readonly IVariantRepository _variantRepository;
        protected readonly IVariantHandler _variantHandler;
        protected readonly IMapper _mapper;

        public VariantService(IVariantRepository VariantRepository, IMapper mapper, IVariantHandler variantHandler)
        {
            _variantRepository = VariantRepository;
            _mapper = mapper;
            _variantHandler = variantHandler;
        }

        public async Task<IQueryable<VariantDTO>> GetAllAsync()
        {
            var entities = await _variantRepository.GetAllAsync(x => !x.IsDeleted);

            return entities.Select(v => new VariantDTO
            {
                Id = v.Id,
                Text = v.Text,
                IsCorrect = v.IsCorrect,
            }).AsQueryable();
        }

        public async Task<VariantDTO> GetByIdAsync(GetByIdVariantDTO dto)
        {
            var entity = _variantHandler.HandleEntityAsync(
                await _variantRepository.GetByIdAsync(x => true));

            return new VariantDTO
            {
                Id = entity.Id,
                Text = entity.Text,
                IsCorrect = entity.IsCorrect,
            };
        }

        public async Task<VariantDTO> AddAsync(CreateVariantDTO dto)
        {
            var entity = await _variantRepository.AddAsync(_mapper.Map<Variant>(dto));

            return new VariantDTO
            {
                Id = entity.Id,
                Text = entity.Text,
                IsCorrect = entity.IsCorrect,
            };
        }

        public async Task<VariantDTO> DeleteAsync(DeleteVariantDTO dto)
        {
            var entity = await _variantRepository.DeleteAsync(
                _variantHandler.HandleEntityAsync(
                await _variantRepository.GetByIdAsync(x => x.Id == dto.Id)));

            return new VariantDTO
            {
                Id = entity.Id,
                Text = entity.Text,
                IsCorrect = entity.IsCorrect,
            };
        }

        public async Task<VariantDTO> UpdateAsync(UpdateVariantDTO dto)
        {
            var oldVariant = await _variantRepository.GetByIdAsync(x => x.Id == dto.Id);

            var map = _mapper.Map(dto,
                _variantHandler.HandleEntityAsync(oldVariant));

            var entity = await _variantRepository.UpdateAsync(map);

            return new VariantDTO
            {
                Id = entity.Id,
                Text = entity.Text,
                IsCorrect = entity.IsCorrect,
            };
        }
    }
}
