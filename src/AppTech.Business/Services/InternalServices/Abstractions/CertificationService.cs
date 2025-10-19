using AppTech.Business.DTOs.CertificationDTOs;
using AppTech.Business.DTOs.PromotionDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Identity;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Repositories.Interfaces;
using AppTech.Shared.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class CertificationService : ICertificationService
    {
        protected readonly ICertificationPromotionRepository _certificationPromotionRepository;
        protected readonly ICertificationUserRepository _certificationUserRepository;
        protected readonly ICertificationRepository _certificationRepository;
        protected readonly IPromotionUserRepository _promotionUserRepository;
        protected readonly ICertificationHandler _certificationHandler;
        protected readonly ICompanyRepository _companyRepository;
        protected readonly UserManager<User> _userManager;
        protected readonly IClaimService _claimService;
        protected readonly IHttpContextAccessor _http;
        protected readonly IMapper _mapper;

        public CertificationService(
            ICertificationRepository certificationRepository,
            IMapper mapper,
            ICertificationHandler certificationHandler,
            UserManager<User> userManager,
            ICompanyRepository companyRepository,
            ICertificationPromotionRepository certificationPromotionRepository,
            IHttpContextAccessor http,
            ICertificationUserRepository certificationUserRepository,
            IClaimService claimService,
            IPromotionUserRepository promotionUserRepository)
        {
            _certificationRepository = certificationRepository;
            _certificationHandler = certificationHandler;
            _mapper = mapper;
            _userManager = userManager;
            _companyRepository = companyRepository;
            _certificationPromotionRepository = certificationPromotionRepository;
            _http = http;
            _certificationUserRepository = certificationUserRepository;
            _claimService = claimService;
            _promotionUserRepository = promotionUserRepository;
        }



        // Get information from database
        public async Task<IQueryable<CertificationUserResponseDTO>> GetAllByUserAsync(GetAllByUserDTO dto)
        {
            var certifications = (await _certificationUserRepository.GetAllAsync(x => !x.IsDeleted,
                u => u.User, c => c.Certification)).Where(x => x.UserId == dto.userId && !x.Certification.IsDeleted);

            var companyIds = certifications.Select(x => x.Certification.CompanyId).Distinct().ToList();

            var companies = await _companyRepository.GetAllAsync(c => companyIds.Contains(c.Id));
            var companyDict = companies.ToDictionary(c => c.Id, c => c.Title);

            return certifications.Select(x => new CertificationUserResponseDTO
            {
                ImageUrl = x.Certification.ImageUrl,
                Title = x.Certification.Title,
                CompanyTitle = companyDict.TryGetValue(x.Certification.CompanyId, out var title)
                ? title : "Unknown Company",
                Slug = x.Certification.Slug,
                LastVersion = x.Certification.LastVersion,
            }).AsQueryable();
        }

        public async Task<CertificationDashboardDTO> GetBySlugByUserAsync(GetBySlugCertificationDTO dto)
        {
            var language = LanguageChanger.Change(new LanguageCatcher(_http).GetLanguage());

            var entity = _certificationHandler.HandleEntityAsync(
                await _certificationRepository.GetByIdAsync(
                    x => x.Slug == dto.slug && !x.IsDeleted,
                    t => t.CertificationTranslations,
                    q => q.Questions));

            return new CertificationDashboardDTO
            {
                Id = entity.Id,
                Slug = entity.Slug,
                Title = entity.Title,
                ImageUrl = entity.ImageUrl,
                QuestionCount = entity.Questions.Count(),
                LastVersion = entity.LastVersion,
                Description = entity.CertificationTranslations
                    .Where(ct => ct.Language == language && !ct.IsDeleted)
                    .Select(ct => ct.Description)
                    .FirstOrDefault(),
            };
        }


        // CRUD Section
        public async Task<IQueryable<CertificationDTO>> GetAllAsync(GetAllCertificationDTO dto)
        {
            var searchTerm = dto.search?.ToLower();

            var entities = (await _certificationRepository.GetAllAsync(
                x => !x.IsDeleted,
                x => x.Company,
                x => x.CertificationTranslations))
                .Where(x => string.IsNullOrEmpty(searchTerm) ||
                            x.Title.ToLower().Contains(searchTerm))
                .GroupBy(x => x.Code)
                .Select(g => g.OrderByDescending(x => x.LastVersion).FirstOrDefault())
                .Select(e => new CertificationDTO
                {
                    Id = e.Id,
                    Slug = e.Slug,
                    Title = e.Title,
                    SubTitle = e.SubTitle,
                    ImageUrl = e.ImageUrl,
                    CompanyId = e.CompanyId,
                    CompanySlug = e.Company.Slug,
                    Code = e.Code,
                    IsTrend = e.IsTrend,
                    LastVersion = e.LastVersion.GetValueOrDefault()
                });

            return entities.AsQueryable();
        }

        public async Task<CertificationDTO> GetByIdAsync(GetByIdCertificationDTO dto)
        {
            var language = LanguageChanger.Change(new LanguageCatcher(_http).GetLanguage());

            var entity = _certificationHandler.HandleEntityAsync(
                await _certificationRepository.GetByIdAsync(
                    x => x.Id == dto.Id,
                    t => t.CertificationTranslations,
                    q => q.Questions));

            var promotions = (await _certificationPromotionRepository
                .GetAllAsync(x => !x.IsDeleted && x.CertificationId == entity.Id,
                p => p.Promotion))
                .Select(p => new PromotionDTO
                {
                    Id = p.PromotionId,
                    Code = p.Promotion.Code,
                    Percentage = p.Promotion.Percentage / 100m
                }).ToList();

            var promotionUsers = await _promotionUserRepository.GetAllAsync(x => !x.IsDeleted);

            var userId = _claimService.GetUserId();

            var certUser = await _certificationUserRepository.GetByIdAsync(
                x => !x.IsDeleted && x.UserId == userId && x.CertificationId == entity.Id,
                u => u.User,
                c => c.Certification);

            promotions = promotions.Where(p => !promotionUsers.Any(pu => pu.UserId == userId && pu.PromotionId == p.Id)).ToList();

            return new CertificationDTO
            {
                Id = entity.Id,
                Code = entity.Code,
                Slug = entity.Slug,
                Title = entity.Title,
                Price = entity.Price,
                Promotions = promotions,
                IsTrend = entity.IsTrend,
                SubTitle = entity.SubTitle,
                ImageUrl = entity.ImageUrl,
                CompanyId = entity.CompanyId,
                IsBought = certUser is not null,
                DiscountPrice = entity.DiscountPrice,
                QuestionCount = entity.Questions.Count,
                LastUpdate = entity.UpdatedOn.ToLocalTime(),
                LastVersion = entity.LastVersion.GetValueOrDefault(),
                Description = entity.CertificationTranslations
                    .Where(ct => ct.Language == language && !ct.IsDeleted)
                    .Select(ct => ct.Description)
                    .FirstOrDefault(),
                Translations = entity.CertificationTranslations.Select(ct => new CertificationTranslationDTO
                {
                    Id = ct.Id,
                    CertificationId = ct.CertificationId,
                    Description = ct.Description,
                    Language = EnumExtensions.EnumToString(ct.Language),
                }).ToList()
            };
        }


        public async Task<CertificationDTO> GetBySlugAsync(GetBySlugCertificationDTO dto)
        {
            var language = LanguageChanger.Change(new LanguageCatcher(_http).GetLanguage());

            var entity = _certificationHandler.HandleEntityAsync(
                await _certificationRepository.GetByIdAsync(
                    x => x.Slug == dto.slug,
                    t => t.CertificationTranslations,
                    q => q.Questions,
                    s => s.Subscriptions));

            var promotions = (await _certificationPromotionRepository
                .GetAllAsync(x => !x.IsDeleted && x.CertificationId == entity.Id,
                p => p.Promotion))
                .Select(p => new PromotionDTO
                {
                    Id = p.PromotionId,
                    Code = p.Promotion.Code,
                    Percentage = p.Promotion.Percentage / 100m
                }).ToList();

            var promotionUsers = await _promotionUserRepository.GetAllAsync(x => !x.IsDeleted);

            var userId = _claimService.GetUserId();
            var userEmail = userId is not null ? (await _userManager.FindByIdAsync(userId)).Email : string.Empty;

            var certUser = await _certificationUserRepository.GetByIdAsync(
                x => !x.IsDeleted && x.UserId == userId && x.CertificationId == entity.Id,
                u => u.User,
                c => c.Certification);

            promotions = promotions.Where(p => !promotionUsers.Any(pu => pu.UserId == userId && pu.PromotionId == p.Id)).ToList();

            var checkSubscription = entity.Subscriptions
                .Where(s => !s.IsDeleted &&
                            (string.IsNullOrEmpty(s.SpecificDomain) || userEmail.EndsWith(s.SpecificDomain)))
                .Select(s => new SubscriptionResponseDTO
                {
                    Id= s.Id,
                    Title = entity.Title,
                    Price = s.Price,
                    ImageUrl = s.ImageUrl,
                }).FirstOrDefault();

            return new CertificationDTO
            {
                Id = entity.Id,
                Slug = entity.Slug,
                Code = entity.Code,
                Title = entity.Title,
                Price = entity.Price,
                Promotions = promotions,
                IsTrend = entity.IsTrend,
                SubTitle = entity.SubTitle,
                ImageUrl = entity.ImageUrl,
                CompanyId = entity.CompanyId,
                IsBought = certUser is not null,
                DiscountPrice = entity.DiscountPrice,
                QuestionCount = entity.Questions.Count,
                Subcription = checkSubscription ?? null,
                LastUpdate = entity.UpdatedOn.ToLocalTime(),
                LastVersion = entity.LastVersion.GetValueOrDefault(),
                Description = entity.CertificationTranslations
                    .Where(ct => ct.Language == language && !ct.IsDeleted)
                    .Select(ct => ct.Description)
                    .FirstOrDefault(),
                Translations = entity.CertificationTranslations.Select(ct => new CertificationTranslationDTO
                {
                    Id = ct.Id,
                    CertificationId = ct.CertificationId,
                    Description = ct.Description,
                    Language = EnumExtensions.EnumToString(ct.Language),
                }).ToList()
            };
        }

        public async Task<CertificationDTO> AddAsync(CreateCertificationDTO dto)
        {
            var newCertification = _mapper.Map<Certification>(dto);
            newCertification.Title = await SetTitleAsync(dto.CompanyId, dto.Code);
            newCertification.Slug = SlugCreator.GenerateSlug($"{dto.Code} {dto.LastVersion}");

            var entity = await _certificationRepository.AddAsync(newCertification);

            return new CertificationDTO
            {
                Id = entity.Id,
                Slug = entity.Slug,
                Code = entity.Code,
                CompanyId = entity.CompanyId,
                DiscountPrice = entity.DiscountPrice,
                Title = entity.Title,
                SubTitle = entity.SubTitle,
                Price = entity.Price,
                ImageUrl = entity.ImageUrl,
                IsTrend = entity.IsTrend
            };
        }

        public async Task<CertificationDTO> DeleteAsync(DeleteCertificationDTO dto)
        {
            var entity = await _certificationRepository.DeleteAsync(
                _certificationHandler.HandleEntityAsync(
                await _certificationRepository.GetByIdAsync(x => x.Id == dto.Id, t => t.CertificationTranslations)));

            return new CertificationDTO
            {
                Id = entity.Id,
                Slug = entity.Slug,
                Code = entity.Code,
                CompanyId = entity.CompanyId,
                Title = entity.Title,
                SubTitle = entity.SubTitle,
                Price = entity.Price,
                DiscountPrice = entity.DiscountPrice,
                ImageUrl = entity.ImageUrl,
                IsTrend = entity.IsTrend,
            };
        }

        public async Task<CertificationDTO> UpdateAsync(UpdateCertificationDTO dto)
        {
            var oldCertification = _mapper.Map(dto,
                _certificationHandler.HandleEntityAsync(
                await _certificationRepository.GetByIdAsync(x => x.Id == dto.Id)));

            oldCertification.Title = await SetTitleAsync(dto.CompanyId, dto.Code);
            oldCertification.Slug = SlugCreator.GenerateSlug($"{oldCertification.Code} {dto.LastVersion}");

            var entity = await _certificationRepository.UpdateAsync(oldCertification);

            return new CertificationDTO
            {
                Id = entity.Id,
                Slug = entity.Slug,
                Code = entity.Code,
                CompanyId = entity.CompanyId,
                Title = entity.Title,
                SubTitle = entity.SubTitle,
                DiscountPrice = entity.DiscountPrice,
                Price = entity.Price,
                ImageUrl = entity.ImageUrl,
                IsTrend = entity.IsTrend,
            };
        }

        public async Task<string> SetTitleAsync(int companyId, string code)
        {
            var company = await _companyRepository.GetByIdAsync(x => x.Id == companyId);

            return $"{company.Title} {code}";
        }
    }
}
