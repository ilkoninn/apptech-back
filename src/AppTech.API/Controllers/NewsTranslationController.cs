using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.NewsDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Business.Validators.NewsValidator;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class NewsTranslationController : APIController
    {
        private readonly INewsTranslationService _newsTranslationService;

        public NewsTranslationController(INewsTranslationService NewsTranslationService)
        {
            _newsTranslationService = NewsTranslationService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _newsTranslationService.GetAllAsync());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = new GetByIdNewsTranslationDTO { Id = id };
            var validations = await new GetByIdNewsTranslationDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _newsTranslationService.GetByIdAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsync([FromForm] CreateNewsTranslationDTO dto)
        {
            var validations = await new CreateNewsTranslationDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _newsTranslationService.AddAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var dto = new DeleteNewsTranslationDTO { Id = id };
            var validations = await new DeleteNewsTranslationDTOValidator().ValidateAsync(dto);
            return validations.IsValid ? Ok(await _newsTranslationService.DeleteAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdateNewsTranslationDTO dto)
        {
            var validations = await new UpdateNewsTranslationDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _newsTranslationService.UpdateAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }
    }
}
