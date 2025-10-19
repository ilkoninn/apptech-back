using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.CompanyDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Business.Validators;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class CompanyTranslationController : APIController
    {
        private readonly ICompanyTranslationService _companyTranslationService;

        public CompanyTranslationController(ICompanyTranslationService CompanyTranslationService)
        {
            _companyTranslationService = CompanyTranslationService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _companyTranslationService.GetAllAsync());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = new GetByIdCompanyTranslationDTO { Id = id };

            return Ok(await _companyTranslationService.GetByIdAsync(dto));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsync([FromForm] CreateCompanyTranslationDTO dto)
        {
            var validations = await new CreateCompanyTranslationDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _companyTranslationService.AddAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var dto = new DeleteCompanyTranslationDTO { Id = id };

            return Ok(await _companyTranslationService.DeleteAsync(dto));
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdateCompanyTranslationDTO dto)
        {
            var validations = await new UpdateCompanyTranslationDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _companyTranslationService.UpdateAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }
    }
}
