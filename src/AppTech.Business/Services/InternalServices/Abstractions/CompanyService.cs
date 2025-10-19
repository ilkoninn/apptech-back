using AppTech.Business.DTOs.CertificationDTOs;
using AppTech.Business.DTOs.CompanyDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class CompanyService : ICompanyService
    {
        protected readonly ICompanyRepository _companyRepository;
        protected readonly ICompanyHandler _companyHandler;
        protected readonly IWebHostEnvironment _environment;
        protected readonly IConfiguration _config;
        protected readonly IHttpContextAccessor _http;
        protected readonly IMapper _mapper;
        public CompanyService(ICompanyRepository companyRepository, IMapper mapper,
            ICompanyHandler companyHandler, IWebHostEnvironment environment, IHttpContextAccessor http)
        {
            _companyRepository = companyRepository;
            _companyHandler = companyHandler;
            _environment = environment;
            _mapper = mapper;
            _http = http;
        }

        public async Task<IQueryable<CompanyDTO>> GetAllAsync(GetAllCompanyDTO dto)
        {
            var query = await _companyRepository.GetAllAsync(
                x => x.IsDeleted == false,
                x => x.Certifications,
                x => x.CompanyTranslations
            );

            return query
                .Where(x => dto.isTop ? x.IsTop == true : true)
                .Select(e => new CompanyDTO
                {
                    Id = e.Id,
                    Slug = e.Slug,
                    Title = e.Title,
                    ImageUrl = e.ImageUrl,
                    IsTop = e.IsTop,
                })
                .AsQueryable(); ;
        }

        public async Task<PaginatedQueryable<CompanyDTO>> GetAllByPaginationAsync(GetAllCompanyByPageDTO dto)
        {
            var query = await _companyRepository.GetAllAsync(
                x => x.IsDeleted == false,
                x => x.Certifications,
                x => x.CompanyTranslations
            );

            var count = query.Count;

            var selectedQuery = query
                .Select(e => new CompanyDTO
                {
                    Id = e.Id,
                    Slug = e.Slug,
                    Title = e.Title,
                    ImageUrl = e.ImageUrl,
                    IsTop = e.IsTop,
                })
                .OrderBy(b => b.Id)
                .Skip((dto.pageIndex - 1) * dto.pageSize)
                .Take(dto.pageSize)
                .AsQueryable();

            var totalPages = (int)Math.Ceiling(count / (double)dto.pageSize);

            return new PaginatedQueryable<CompanyDTO>(selectedQuery, dto.pageIndex, totalPages);
        }

        public async Task<CompanyDTO> GetByIdAsync(GetByIdCompanyDTO dto)
        {
            var language = LanguageChanger.Change(new LanguageCatcher(_http).GetLanguage());
            var entity = _companyHandler.HandleEntityAsync(
                await _companyRepository.GetByIdAsync(
                    x => x.Id == dto.Id,
                    x => x.Certifications,
                    x => x.CompanyTranslations
                )
            );

            return new CompanyDTO
            {
                Id = entity.Id,
                Slug = entity.Slug,
                Title = entity.Title,
                ImageUrl = entity.ImageUrl,
                IsTop = entity.IsTop,
                SecTitle = entity.CompanyTranslations
                    .Where(ct => ct.Language == language && !ct.IsDeleted)
                    .Select(ct => ct.SecTitle)
                    .FirstOrDefault(),
                SecDescription = entity.CompanyTranslations
                    .Where(ct => ct.Language == language && !ct.IsDeleted)
                    .Select(ct => ct.SecDescription)
                    .FirstOrDefault(),
                Translations = entity.CompanyTranslations.Select(ct => new CompanyTranslationDTO
                {
                    Id = ct.Id,
                    CompanyId = ct.CompanyId,
                    SecTitle = ct.SecTitle,
                    SecDescription = ct.SecDescription,
                    Language = EnumExtensions.EnumToString(ct.Language),
                }).ToList()
            };
        }

        public async Task<CompanyDTO> GetBySlugAsync(GetBySlugCompanyDTO dto)
        {
            var language = LanguageChanger.Change(new LanguageCatcher(_http).GetLanguage());
            var entity = _companyHandler.HandleEntityAsync(
                await _companyRepository.GetByIdAsync(
                    x => x.Slug == dto.slug,
                    x => x.Certifications,
                    x => x.CompanyTranslations
                )
            );

            return new CompanyDTO
            {
                Id = entity.Id,
                Slug = entity.Slug,
                Title = entity.Title,
                ImageUrl = entity.ImageUrl,
                IsTop = entity.IsTop,
                SecTitle = entity.CompanyTranslations
                    .Where(ct => ct.Language == language && !ct.IsDeleted)
                    .Select(ct => ct.SecTitle)
                    .FirstOrDefault(),
                SecDescription = entity.CompanyTranslations
                    .Where(ct => ct.Language == language && !ct.IsDeleted)
                    .Select(ct => ct.SecDescription)
                    .FirstOrDefault(),
                CertificationDTOs = entity.Certifications
                    .Where(x => !x.IsDeleted)
                    .Select(ce => new CertificationDTO
                    {
                        Id = ce.Id,
                        Slug = ce.Slug,
                        Title = ce.Title,
                        SubTitle = ce.SubTitle,
                        ImageUrl = ce.ImageUrl,
                        CompanyId = ce.CompanyId,
                        Code = ce.Code,
                    }).ToList(),
            };
        }

        public async Task<CompanyDTO> AddAsync(CreateCompanyDTO dto)
        {
            var newCompany = _mapper.Map<Company>(dto);
            newCompany.Slug = SlugCreator.GenerateSlug(dto.Title);

            var entity = await _companyRepository.AddAsync(newCompany);

            return new CompanyDTO
            {
                Id = entity.Id,
                Slug = entity.Slug,
                Title = entity.Title,
                ImageUrl = entity.ImageUrl,
                IsTop = entity.IsTop,
            };
        }

        public async Task<CompanyDTO> DeleteAsync(DeleteCompanyDTO dto)
        {
            var entity = await _companyRepository.DeleteAsync(
                _companyHandler.HandleEntityAsync(
                await _companyRepository.GetByIdAsync(x => x.Id == dto.Id, c => c.Certifications, x => x.CompanyTranslations)));


            return new CompanyDTO
            {
                Id = entity.Id,
                Slug = entity.Slug,
                Title = entity.Title,
                ImageUrl = entity.ImageUrl,
                IsTop = entity.IsTop,
            };
        }

        public async Task<CompanyDTO> UpdateAsync(UpdateCompanyDTO dto)
        {
            var oldCompany = _mapper.Map(dto,
                _companyHandler.HandleEntityAsync(
                await _companyRepository.GetByIdAsync(x => x.Id == dto.Id)));

            oldCompany.Slug = SlugCreator.GenerateSlug(dto.Title ?? oldCompany.Title);

            var entity = await _companyRepository.UpdateAsync(oldCompany);

            return new CompanyDTO
            {
                Id = entity.Id,
                Slug = entity.Slug,
                Title = entity.Title,
                ImageUrl = entity.ImageUrl,
                IsTop = entity.IsTop,
            };
        }
    }
}
