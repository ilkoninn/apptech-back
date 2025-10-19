using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.VariantDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Business.Validators.QuestionValidator;
using AppTech.Business.Validators.VariantValidator;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class VariantController : APIController
    {
        private readonly IVariantService _variantService;

        public VariantController(IVariantService VariantService)
        {
            _variantService = VariantService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _variantService.GetAllAsync());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = new GetByIdVariantDTO() { Id = id };
            var validations = await new GetByIdVariantDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _variantService.GetByIdAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsync([FromForm] CreateVariantDTO dto)
        {
            var validations = await new CreateVariantDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _variantService.AddAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });

        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var dto = new DeleteVariantDTO() { Id = id };
            var validations = await new DeleteVariantDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _variantService.DeleteAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdateVariantDTO dto)
        {
            var validations = await new UpdateVariantDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _variantService.UpdateAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }
    }
}
