using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.QuestionDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Business.Validators.QuestionValidator;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class QuestionController : APIController
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService QuestionService)
        {
            _questionService = QuestionService;
        }

        [HttpGet("{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllQuestionWithVariantsAsync(string slug)
        {
            var dto = new GetAllBySlugDTO { slug = slug };

            return Ok(await _questionService.GetAllQuestionWithVariantsAsync(dto));
        }

        [HttpPost("report")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReportQuestionAsync([FromBody] ReportQuestionDTO dto)
        {
            await _questionService.ReportTheSelectedQuestionAsync(dto);

            return Ok(new { message = "The question has been successfully reported." });
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _questionService.GetAllAsync());
        }

        [HttpGet("id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = new GetByIdQuestionDTO() { Id = id };
            var validations = await new GetByIdQuestionDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _questionService.GetByIdAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsync([FromForm] CreateQuestionDTO dto)
        {
            var validations = await new CreateQuestionDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _questionService.AddAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var dto = new DeleteQuestionDTO() { Id = id };
            var validations = await new DeleteQuestionDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _questionService.DeleteAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateQuestion([FromForm] UpdateQuestionDTO dto)
        {
            var validations = await new UpdateQuestionDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _questionService.UpdateAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

    }
}
