// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.CertificationDTOs;
using AppTech.Business.DTOs.SearchDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Validators.CertificationValidator;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Identity;
using AppTech.Core.Enums;
using AppTech.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace AppTech.API.Controllers
{
    public class SearchController : APIController
    {
        private readonly ICertificationUserRepository _certificationUserRepository;
        private readonly IHttpContextAccessor _http;
        private readonly UserManager<User> _userManager;
        private readonly ICompanyRepository _companyRepository;
        private readonly IExamRepository _examRepository;

        public SearchController(IExamRepository examRepository, ICompanyRepository companyRepository, ICertificationUserRepository certificationUserRepository, UserManager<User> userManager, IHttpContextAccessor http)
        {
            _examRepository = examRepository;
            _companyRepository = companyRepository;
            _certificationUserRepository = certificationUserRepository;
            _userManager = userManager;
            _http = http;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync([FromQuery] SearchDTO dto)
        {
            var certificationQuery = await _certificationUserRepository
                .GetAllAsync(x => !x.IsDeleted && x.UserId == dto.userId, c => c.Certification, u => u.User);

            var oldUser = await _userManager.Users
               .Include(x => x.ExamResults)
                   .ThenInclude(er => er.Exam)
                       .ThenInclude(e => e.Certification)
                           .ThenInclude(c => c.Company)
               .Where(x => x.Id == dto.userId)
               .FirstOrDefaultAsync();

            var boughtExams = oldUser?.ExamResults.Select(x => x.Exam).Distinct().ToList() ?? new List<Exam>();

            var boughtExamIds = boughtExams.Select(x => x.Id).ToList();

            var allExams = await _examRepository.GetAllAsync(x => !x.IsDeleted, c => c.Certification);

            var companies = await _companyRepository.GetAllAsync(x => !x.IsDeleted);

            var combinedResults = new List<ResponseSearchDTO>();

            var language = new LanguageCatcher(_http).GetLanguage();

            combinedResults.AddRange(certificationQuery.Select(x => new ResponseSearchDTO
            {
                Type = language switch
                {
                    "az" => "Material",
                    "ru" => "Материал",
                    _ => "Material"
                },
                Title = x.Certification.Title,
                SubTitle = x.Certification.SubTitle,
                Url = $"/dashboard/products/materials/{x.Certification.Slug}",
                ImageUrl = x.Certification.ImageUrl,
                LastVersion = x.Certification.LastVersion,
            }));

            combinedResults.AddRange(boughtExams.Select(x => new ResponseSearchDTO
            {
                Type = language switch
                {
                    "az" => "İmtahanım",
                    "ru" => "Ваш экзамен",
                    _ => "Own Exam"
                },
                Title = x.Certification.Title,
                SubTitle = x.Code,
                Url = $"/dashboard/products/exams/{x.Slug}", 
                ImageUrl = x.Certification?.ImageUrl,
                LastVersion = x.Certification?.LastVersion, 
            }));

            combinedResults.AddRange(allExams.Select(x => new ResponseSearchDTO
            {
                Type = language switch
                {
                    "az" => "İmtahan",
                    "ru" => "Экзамен",
                    _ => "Exam"
                },
                Title = x.Certification.Title,
                SubTitle = x.Code,
                Url = boughtExamIds.Contains(x.Id)
                    ? $"/dashboard/take-the-exam/{x.Certification.Slug}/{x.Code}"
                    : $"/dashboard/products/exams/{x.Slug}", 
                ImageUrl = x.Certification?.ImageUrl,
                LastVersion = x.Certification?.LastVersion,
            }));

            combinedResults.AddRange(companies.Select(x => new ResponseSearchDTO
            {
                Type = language switch
                {
                    "az" => "Təchizatçı",
                    "ru" => "Поставщик",
                    _ => "Provider"
                },
                Title = x.Title,
                Url = $"/dashboard/take-the-exam/{x.Slug}",
                ImageUrl = x.ImageUrl
            }));

            if (!string.IsNullOrEmpty(dto.search))
            {
                combinedResults = combinedResults
                    .Where(x => x.Title.Contains(dto.search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return Ok(combinedResults);
        }
    }
}
