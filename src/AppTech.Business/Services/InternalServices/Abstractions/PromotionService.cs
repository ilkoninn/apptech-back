using AppTech.Business.DTOs.PromotionDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class PromotionService : IPromotionService
    {
        protected readonly IPromotionRepository _promotionRepository;
        protected readonly ICertificationPromotionRepository _certificationPromotionRepository;
        protected readonly IPromotionHandler _promotionHandler;
        protected readonly IMapper _mapper;
        protected readonly AppDbContext _appDbContext;
        public PromotionService(IPromotionRepository PromotionRepository,
            IMapper mapper, AppDbContext appDbContext,
            ICertificationPromotionRepository certificationPromotionRepository,
            IPromotionHandler promotionHandler)
        {
            _promotionRepository = PromotionRepository;
            _mapper = mapper;
            _appDbContext = appDbContext;
            _certificationPromotionRepository = certificationPromotionRepository;
            _promotionHandler = promotionHandler;
        }

        public async Task<IQueryable<PromotionDTO>> GetAllAsync()
        {
            var query = await _promotionRepository.GetAllAsync(x => x.IsDeleted == false,
                cp => cp.CertificationPromotions);

            return query.Select(p => new PromotionDTO
            {
                Id = p.Id,
                Code = p.Code,
                Percentage = p.Percentage
            }).AsQueryable();
        }

        public async Task<PromotionDTO> GetByIdAsync(GetByIdPromotionDTO dto)
        {
            var entity = _promotionHandler.HandleEntityAsync(
                await _promotionRepository.GetByIdAsync(
                    x => x.Id == dto.Id, x => x.CertificationPromotions));

            return new PromotionDTO
            {
                Id = entity.Id,
                Code = entity.Code,
                Percentage = entity.Percentage
            };
        }

        public async Task<PromotionDTO> AddAsync(CreatePromotionDTO dto)
        {
            var promotion = new Promotion
            {
                Code = dto.Code,
                EndedOn= dto.EndedOn,
                Percentage = dto.Percentage,
                CreatedOn = DateTime.UtcNow,
                CertificationPromotions = new List<CertificationPromotion>()
            };

            foreach (var certificationId in dto.CertificationIds)
            {
                var certificationPromotion = new CertificationPromotion
                {
                    CertificationId = certificationId,
                    Promotion = promotion,
                    CreatedOn = DateTime.UtcNow
                };  

                await _certificationPromotionRepository.AddAsync(certificationPromotion);
            }

            var entity = promotion;

            return new PromotionDTO
            {
                Id = entity.Id,
                Code = entity.Code,
                Percentage = entity.Percentage
            };
        }


        public async Task<PromotionDTO> DeleteAsync(int id)
        {
            var entity = await _promotionRepository.DeleteAsync(
                _promotionHandler.HandleEntityAsync(
                await _promotionRepository.GetByIdAsync(x => x.Id == id)));

            return new PromotionDTO
            {
                Id = entity.Id,
                Code = entity.Code,
                Percentage = entity.Percentage
            };
        }

        public async Task<PromotionDTO> UpdateAsync(UpdatePromotionDTO dto)
        {
            var entity = await _promotionRepository.GetByIdAsync(
                x => x.Id == dto.Id, x => x.CertificationPromotions);
            entity.CertificationPromotions.Clear();
            foreach (var certificationId in dto.CertificationIds)
            {
                entity.CertificationPromotions.Add(new CertificationPromotion
                {
                    CertificationId = certificationId,
                    PromotionId = entity.Id
                });
            }
             _mapper.Map(dto, entity);
             var updatedEntity = await _promotionRepository.UpdateAsync(entity);
             return new PromotionDTO
            {
                Id = updatedEntity.Id,
                Code = updatedEntity.Code,
                Percentage = updatedEntity.Percentage
            };
        }


    }
}
