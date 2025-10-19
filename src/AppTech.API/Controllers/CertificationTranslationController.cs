using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.CertificationDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Business.Validators.CertificationValidator;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class CertificationTranslationController : APIController
    {
        private readonly ICertificationTranslationService _certificationTranslationService;

        public CertificationTranslationController(ICertificationTranslationService certificationTranslationService)
        {
            _certificationTranslationService = certificationTranslationService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _certificationTranslationService.GetAllAsync());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = new GetByIdCertificationTranslationDTO { Id = id };

            return Ok(await _certificationTranslationService.GetByIdAsync(dto));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsync([FromForm] CreateCertificationTranslationDTO dto)
        {
            var validations = await new CreateCertificationTranslationDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _certificationTranslationService.AddAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var dto = new DeleteCertificationTranslationDTO { Id = id };

            return Ok(await _certificationTranslationService.DeleteAsync(dto));
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdateCertificationTranslationDTO dto)
        {
            var validations = await new UpdateCertificationTranslationDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _certificationTranslationService.UpdateAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }
    }
}
