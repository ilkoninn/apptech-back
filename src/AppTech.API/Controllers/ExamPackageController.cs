using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.ExamPackageDTOs;
using AppTech.Business.Services.InternalServices.Interfaces;
using AppTech.Business.Validators.ExamPackageValidator;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class ExamPackageController : APIController
    {
        private readonly IExamPackageService _examPackageService;

        public ExamPackageController(IExamPackageService faqService)
        {
            _examPackageService = faqService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync([FromQuery] GetAllExamPackageDTO dto)
        {
            return Ok(await _examPackageService.GetAllAsync(dto));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = new GetByIdExamPackageDTO { Id = id };
            var validations = await new GetByIdExamPackageDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _examPackageService.GetByIdAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsync([FromForm] CreateExamPackageDTO dto)
        {
            var validations = await new CreateExamPackageDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _examPackageService.AddAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var dto = new DeleteExamPackageDTO { Id = id };
            var validations = await new DeleteExamPackageDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _examPackageService.DeleteAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdateExamPackageDTO dto)
        {
            var validations = await new UpdateExamPackageDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _examPackageService.UpdateAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }
    }
}
