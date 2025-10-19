using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.SettingDTOs;
using AppTech.Business.Services.InternalServices.Interfaces;
using AppTech.Business.Validators.SettingValidator;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingTranslationController : APIController
    {
        private readonly ISettingTranslationService _settingTranslationService;

        public SettingTranslationController(ISettingTranslationService settingTranslationService)
        {
            _settingTranslationService = settingTranslationService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _settingTranslationService.GetAllAsync());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = new GetByIdSettingTranslationDTO() { Id = id };
            var validations = await new GetByIdSettingTranslationDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _settingTranslationService.GetByIdAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsync([FromForm] CreateSettingTranslationDTO dto)
        {
            var validations = await new CreateSettingTranslationDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _settingTranslationService.AddAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var dto = new DeleteSettingTranslationDTO() { Id = id };
            var validations = await new DeleteSettingTranslationDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _settingTranslationService.DeleteAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdateSettingTranslationDTO dto)
        {
            var validations = await new UpdateSettingTranslationDTOValidator().ValidateAsync(dto);

            return validations.IsValid ?
                Ok(await _settingTranslationService.UpdateAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }
    }
}
