using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.ExamDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Business.Validators.ExamValidator;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamTranslationController : APIController
    {
        private readonly IExamTranslationService _examTranslationService;

        public ExamTranslationController(IExamTranslationService ExamTranslationService)
        {
            _examTranslationService = ExamTranslationService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _examTranslationService.GetAllAsync());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync([FromQuery] GetByIdExamTranslationDTO dto)
        {
            var validations = await new GetByIdExamTranslationDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _examTranslationService.GetByIdAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsync([FromForm] CreateExamTranslationDTO dto)
        {
            var validations = await new CreateExamTranslationDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _examTranslationService.AddAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync([FromForm] DeleteExamTranslationDTO dto)
        {
            var validations = await new DeleteExamTranslationDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _examTranslationService.DeleteAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdateExamTranslationDTO dto)
        {
            var validations = await new UpdateExamTranslationDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _examTranslationService.UpdateAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }
    }
}
