using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.CompanyDTOs;
using AppTech.Business.DTOs.ExamDTOs;
using AppTech.Business.DTOs.UserDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Business.Validators.ExamValidator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class ExamController : APIController
    {
        private readonly IExamService _examService;

        public ExamController(IExamService ExamService)
        {
            _examService = ExamService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync([FromQuery] GetExamBySlugCompanyDTO dto)
        {
            return Ok(await _examService.GetAllAsync(dto));
        }

        [HttpGet("results/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllExamResultsAsync(string userId)
        {
            return Ok(await _examService.GetAllExamResultsByUserAsync(new UserTimerDTO { userId = userId }));
        }

        [HttpGet("id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync([FromQuery] GetByIdExamDTO dto)
        {
            var validations = await new GetByIdExamDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _examService.GetByIdAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpGet("{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBySlugAsync(string slug)
        {
            var dto = new GetBySlugExamDTO { slug = slug };

            return Ok(await _examService.GetBySlugAsync(dto));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsync([FromForm] CreateExamDTO dto)
        {
            var validations = await new CreateExamDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _examService.AddAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });

        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync([FromForm] DeleteExamDTO dto)
        {
            var validations = await new DeleteExamDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _examService.DeleteAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdateExamDTO dto)
        {
            var validations = await new UpdateExamDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _examService.UpdateAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        // User Exam Relational Methods
        [HttpGet("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllByUserIdAsync(string userId)
        {
            var dto = new GetAllExamByUser { userId = userId };

            return Ok(await _examService.GetAllExamsByUser(dto));
        }

        // Exam Process Methods
        [HttpPost("take-exam")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> TakeExamAsync([FromBody] CreateRandomExamDTO dto)
        {
            return Ok(await _examService.TakeExamAsync(dto));
        }

        [HttpPost("start-exam")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> StartExamStatusAsync([FromBody] UserTimerDTO dto)
        {
            await _examService.StartExamAsync(dto);

            return Ok();
        }

        [HttpPost("end-exam")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EndExamStatusAsync([FromBody] UserTimerDTO dto)
        {
            await _examService.EndExamAsync(dto);

            return Ok();
        }

        [HttpPost("check-exam")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CheckExamAsync([FromBody] SubmitExamDTO dto)
        {
            return Ok(await _examService.CheckExamAsync(dto));
        }

        [HttpPost("check-exam-status")]
        public async Task<IActionResult> CheckExamStatusAsync([FromBody] UserTimerDTO dto)
        {
            var result = await _examService.CheckExamStatus(dto.userId);

            return Ok(new { status = result });
        }

        [HttpPost("timer")]
        public async Task<IActionResult> UserTimerAsync([FromBody] UserTimerDTO dto)
        {
            await _examService.ChangeExamStatus(dto.userId);

            return Ok(new { message = "Timer successfully updated." });
        }
    }
}
