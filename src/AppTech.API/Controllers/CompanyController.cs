using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.CompanyDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Business.Validators;
using AppTech.Business.Validators.CompanyValidator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class CompanyController : BaseAPIController
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync([FromQuery] GetAllCompanyDTO dto)
        {
            var validations = await new GetAllCompanyDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _companyService.GetAllAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllByPaginationAsync([FromQuery] GetAllCompanyByPageDTO dto)
        {
            return Ok(await _companyService.GetAllByPaginationAsync(dto));
        }

        [HttpGet("id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = new GetByIdCompanyDTO { Id = id };

            return Ok(await _companyService.GetByIdAsync(dto));
        }

        [HttpGet("{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBySlugAsync(string slug)
        {
            var dto = new GetBySlugCompanyDTO { slug = slug };

            return Ok(await _companyService.GetBySlugAsync(dto));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsync([FromForm] CreateCompanyDTO dto)
        {
            var validations = await new CreateCompanyDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _companyService.AddAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var dto = new DeleteCompanyDTO() { Id = id };

            return Ok(await _companyService.DeleteAsync(dto));
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdateCompanyDTO dto)
        {
            var validations = await new UpdateCompanyDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _companyService.UpdateAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }
    }
}
