using AppTech.Business.DTOs.ExamPackageDTOs;
using AppTech.Business.Services.InternalServices.Interfaces;
using AppTech.Core.Entities;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class ExamPackageService : IExamPackageService
    {
        protected readonly IExamPackageRepository _examPackageRepository;
        protected readonly IExamRepository _examRepository;
        protected readonly IExamPackageHandler _examPackageHandler;
        protected readonly IHttpContextAccessor _http;
        protected readonly IMapper _mapper;

        public ExamPackageService(IExamPackageRepository faqRepository, IMapper mapper,
            IConfiguration configuration, IExamPackageHandler fAQHandler, IHttpContextAccessor http, IExamRepository examRepository)
        {
            _examPackageRepository = faqRepository;
            _mapper = mapper;
            _examPackageHandler = fAQHandler;
            _http = http;
            _examRepository = examRepository;
        }

        public async Task<IQueryable<ExamPackageDTO>> GetAllAsync(GetAllExamPackageDTO dto)
        {
            var domain = dto.email.Split('@').Last();

            // Fetch packages based on the domain
            var examPackages = await _examPackageRepository.GetAllAsync(
                 x => !x.IsDeleted &&
                 (string.IsNullOrEmpty(x.SpecificDomain) ||
                 (!string.IsNullOrEmpty(dto.email) && domain == x.SpecificDomain)),
                 x => x.Exams);

            var examIds = examPackages.SelectMany(x => x.Exams).Select(e => e.Id).ToList();

            var exams = await _examRepository.GetAllAsync(
                x => examIds.Contains(x.Id),
                x => x.Certification);

            var examPackagesDto = examPackages.Select(e => new ExamPackageDTO
            {
                Id = e.Id,
                Price = e.Price,
                ImageUrl = e.ImageUrl,
                Title = e.Title,
                CertificationTitles = e.Exams
                    .Select(ex => exams.FirstOrDefault(x => x.Id == ex.Id)?.Certification?.Title)
                    .Where(ct => !string.IsNullOrEmpty(ct))
            });

            return examPackagesDto.AsQueryable();
        }

        public async Task<ExamPackageDTO> GetByIdAsync(GetByIdExamPackageDTO dto)
        {
            var entity = _examPackageHandler.HandleEntityAsync(
                await _examPackageRepository.GetByIdAsync(x => x.Id == dto.Id));

            return new ExamPackageDTO
            {
                Id = entity.Id,
            };
        }

        public async Task<ExamPackageDTO> AddAsync(CreateExamPackageDTO dto)
        {
            var map = _mapper.Map<ExamPackage>(dto);

            if (dto.ExamIds != null && dto.ExamIds.Any())
            {
                var exams = await _examRepository.GetAllAsync(x => dto.ExamIds.Contains(x.Id));
                map.Exams = exams;
            }

            var entity = await _examPackageRepository.AddAsync(map);

            return new ExamPackageDTO
            {
                Id = entity.Id,
                Title = entity.Title,
                ImageUrl = entity.ImageUrl,
                Price = entity.Price
            };
        }

        public async Task<ExamPackageDTO> DeleteAsync(DeleteExamPackageDTO dto)
        {
            var entity = await _examPackageRepository.DeleteAsync(
                _examPackageHandler.HandleEntityAsync(
                await _examPackageRepository.GetByIdAsync(x => x.Id == dto.Id)));

            return new ExamPackageDTO
            {
                Id = entity.Id,
                Title = entity.Title,
                ImageUrl = entity.ImageUrl,
                Price = entity.Price,
            };
        }

        public async Task<ExamPackageDTO> UpdateAsync(UpdateExamPackageDTO dto)
        {
            var oldExamPackage = _examPackageHandler.HandleEntityAsync(
                await _examPackageRepository.GetByIdAsync(x => x.Id == dto.Id, e => e.Exams));

            var map = _mapper.Map(dto, oldExamPackage);

            if (dto.ExamIds != null && dto.ExamIds.Any())
            {
                var exams = await _examRepository.GetAllAsync(x => dto.ExamIds.Contains(x.Id));
                map.Exams = exams;
            }

            var entity = await _examPackageRepository.UpdateAsync(map);

            return new ExamPackageDTO
            {
                Id = entity.Id,
                Title = entity.Title,
                ImageUrl = entity.ImageUrl,
                Price = entity.Price,
            };
        }
    }
}
