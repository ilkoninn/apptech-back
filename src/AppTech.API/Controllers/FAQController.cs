using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.FAQDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Business.Validators.FAQDTO;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class FAQController : BaseAPIController
    {
        private readonly IFAQService _faqService;

        public FAQController(IFAQService faqService)
        {
            _faqService = faqService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            var dto = new GetAllFAQDTO();

            return Ok(await _faqService.GetAllAsync(dto));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = new GetByIdFAQDTO { Id = id };
            var validations = await new GetByIdFAQDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _faqService.GetByIdAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsync([FromForm] CreateFAQDTO dto)
        {
            var validations = await new CreateFAQDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _faqService.AddAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var dto = new DeleteFAQDTO { Id = id };
            var validations = await new DeleteFAQDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _faqService.DeleteAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdateFAQDTO dto)
        {
            var validations = await new UpdateFAQDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _faqService.UpdateAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }
    }
}
