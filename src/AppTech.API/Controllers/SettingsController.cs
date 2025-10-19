using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.SettingDTOs;
using AppTech.Business.Services.InternalServices.Interfaces;
using AppTech.Business.Validators.SettingValidator;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class SettingController : APIController
    {
        private readonly ISettingService _settingService;

        public SettingController(ISettingService SettingService)
        {
            _settingService = SettingService;
        }

        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            var dto = new SettingTypeDTO { type = "test" };

            return Ok(await _settingService.GetAllAsync(dto));
        }

        [HttpGet("id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = new GetByIdSettingDTO() { Id = id };
            var validations = await new GetByIdSettingDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _settingService.GetByIdAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsync([FromQuery] CreateSettingDTO dto)
        {
            var validations = await new CreateSettingDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _settingService.AddAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var dto = new DeleteSettingDTO() { Id = id };
            var validations = await new DeleteSettingDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _settingService.DeleteAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromQuery] UpdateSettingDTO dto)
        {
            var validations = await new UpdateSettingDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _settingService.UpdateAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }
    }
}

